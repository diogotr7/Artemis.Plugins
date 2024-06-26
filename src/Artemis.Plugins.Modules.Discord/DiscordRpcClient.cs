using Artemis.Core;
using Artemis.Plugins.Modules.Discord.DiscordPackets;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;
using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Plugins.Modules.Discord.Authentication;
using Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;
using Artemis.Plugins.Modules.Discord.Transport;

namespace Artemis.Plugins.Modules.Discord;

public class DiscordRpcClient : IDiscordRpcClient
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
        }
    };

    private readonly Dictionary<Guid, TaskCompletionSource<DiscordResponse>> _pendingRequests;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IDiscordAuthClient _authClient;
    private readonly IDiscordTransport _transport;
    private Task? _readLoopTask;
    private TaskCompletionSource<Ready>? _readyTcs;

    #region Events

    /// <summary>
    /// Sent when an error occurs.
    /// </summary>
    public event EventHandler<DiscordRpcClientException>? Error;

    /// <summary>
    /// Generic event received
    /// </summary>
    public event EventHandler<DiscordEvent>? UnhandledEventReceived;

    /// <summary>
    /// Sent when the client is authenticated and ready to use.
    /// </summary>
    public event EventHandler<Authenticate>? Authenticated;

    /// <summary>
    /// Sent when the client's voice settings update.
    /// </summary>
    public event EventHandler<VoiceSettings>? VoiceSettingsUpdated;

    /// <summary>
    /// Sent when the client's voice connection status changes.
    /// </summary>
    public event EventHandler<VoiceConnectionStatus>? VoiceConnectionStatusUpdated;

    /// <summary>
    /// Sent when the client receives a notification (mention or new message in eligible channels)
    /// </summary>
    public event EventHandler<Notification>? NotificationReceived;

    /// <summary>
    /// Sent when a user in a subscribed voice channel speaks.
    /// </summary>
    public event EventHandler<SpeakingStartStop>? SpeakingStarted;

    /// <summary>
    /// Sent when a user in a subscribed voice channel stops speaking.
    /// </summary>
    public event EventHandler<SpeakingStartStop>? SpeakingStopped;

    /// <summary>
    /// Sent when the client joins (or leaves)  a voice channel.
    /// </summary>
    public event EventHandler<VoiceChannelSelect>? VoiceChannelUpdated;

    /// <summary>
    /// Sent when a user joins a subscribed voice channel.
    /// </summary>
    public event EventHandler<UserVoiceState>? VoiceStateCreated;

    /// <summary>
    /// Sent when a user's voice state changes in a subscribed voice channel (mute, volume, etc.).
    /// </summary>
    public event EventHandler<UserVoiceState>? VoiceStateUpdated;

    /// <summary>
    /// Sent when a user parts a subscribed voice channel.
    /// </summary>
    public event EventHandler<UserVoiceState>? VoiceStateDeleted;

    #endregion

    public DiscordRpcClient(PluginSettings settings)
    {
        _pendingRequests = new Dictionary<Guid, TaskCompletionSource<DiscordResponse>>();
        _cancellationTokenSource = new CancellationTokenSource();
        var rpcType = settings.GetSetting("DiscordRpcProvider", DiscordRpcProvider.StreamKit);
        
        //TODO: sometimes subscribing to NOTIFICATION_CREATE throws an error. investigate.
        // it might be dependent on the transport used, not 100% sure.
        _authClient = rpcType.Value switch
        {
            DiscordRpcProvider.StreamKit => new StreamKitAuthClient(settings),
            DiscordRpcProvider.Custom => new DiscordAuthClient(settings),
            DiscordRpcProvider.Razer => new RazerAuthClient(settings),
            DiscordRpcProvider.Steelseries => new SteelseriesAuthClient(settings),
            DiscordRpcProvider.Logitech => new LogitechAuthClient(settings),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        // The Custom provider does not support the websocket transport.
        _transport = rpcType.Value switch
        {
            DiscordRpcProvider.Custom => new DiscordPipeTransport(_authClient.ClientId),
            _ => new DiscordWebSocketTransport(_authClient.ClientId, _authClient.Origin)
        };
    }

    public async Task Connect(int timeoutMs = 500)
    {
        if (_transport.IsConnected)
            throw new InvalidOperationException("Already connected");

        //we want discord to be open for at least 10 seconds 
        //when we connect. if it hasn't, sleep until then.
        var startTime = GetDiscordStartTime();
        var now = DateTime.Now;
        var desiredTime = startTime + TimeSpan.FromSeconds(10);
        if (now < desiredTime)
            await Task.Delay(desiredTime - now);

        await _transport.Connect(_cancellationTokenSource.Token);
        _readLoopTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);
        //fire and forget, it should be fiiiiine
        _ = Task.Run(InitializeAsync, _cancellationTokenSource.Token);
    }

    public async Task SubscribeAsync(DiscordRpcEvent evt, params (string Key, object Value)[] parameters)
    {
        var subscribe = new DiscordSubscribe(evt, parameters);

        await SendRequestWithResponseTypeAsync<Subscription>(subscribe);
    }

    public async Task UnsubscribeAsync(DiscordRpcEvent evt, params (string Key, object Value)[] parameters)
    {
        var subscribe = new DiscordUnsubscribe(evt, parameters);

        await SendRequestWithResponseTypeAsync<Subscription>(subscribe);
    }

    public async Task<T> GetAsync<T>(DiscordRpcCommand command, params (string Key, object Value)[] parameters) where T : class
    {
        var request = new DiscordRequest(command, parameters);

        var response = await SendRequestWithResponseTypeAsync<T>(request);

        return response.Data;
    }

    private async Task InitializeAsync()
    {
        _readyTcs = new TaskCompletionSource<Ready>();

        //this task will complete once the Ready event is received
        await _readyTcs.Task;

        var authenticatedData = await HandleAuthenticationAsync();

        Authenticated?.Invoke(this, authenticatedData);
    }

    private async Task AuthorizeAsync()
    {
        var authorizeResponse = await SendRequestWithResponseTypeAsync<Authorize>(
            new DiscordRequest(DiscordRpcCommand.AUTHORIZE,
                ("client_id", _authClient.ClientId),
                ("prompt", "none"),
                ("scopes", new[] { "rpc", "identify", "rpc.notifications.read" })),
            timeoutMs: 30000); //high timeout so the user has time to click the button

        await _authClient.GetAccessTokenAsync(authorizeResponse.Data.Code);
    }
    
    public async Task<Authenticate> AuthenticateAsync()
    {
        var authenticateResponse = await SendRequestWithResponseTypeAsync<Authenticate>(
            new DiscordRequest(DiscordRpcCommand.AUTHENTICATE,
                ("access_token", _authClient.AccessToken!))
        );
        
        return authenticateResponse.Data;
    }

    private async Task<Authenticate> HandleAuthenticationAsync()
    {
        if (string.IsNullOrWhiteSpace(_authClient.AccessToken))
        {
            //We have no token saved. This means it's probably the first time
            //the user is using the plugin. We need to ask for their permission
            //to get a token from discord. 
            await AuthorizeAsync();
        }

        try
        {
            return await AuthenticateAsync();
        }
        catch (DiscordException)//this is not the IO exception, it's thrown when discord responds with an error
        {
            //something is wrong with our token, authorize again
            await AuthorizeAsync();
            return await AuthenticateAsync();
        }
    }

    private async Task ReadLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _transport.IsConnected)
        {
            try
            {
                var (opCode, data) = await _transport.ReadMessageAsync(_cancellationTokenSource.Token);

                await ProcessMessageAsync(opCode, data);
            }
            catch (Exception e)
            {
                Error?.Invoke(this, new DiscordRpcClientException("Error in Discord read loop", e, true));
            }
        }
    }

    private async Task ProcessMessageAsync(RpcPacketType opCode, string data)
    {
        if (opCode == RpcPacketType.PING)
        {
            await _transport.SendPacketAsync(data, RpcPacketType.PONG, _cancellationTokenSource.Token);
            return;
        }

        if (opCode == RpcPacketType.CLOSE)
        {
            Error?.Invoke(this, new DiscordRpcClientException($"Discord sent RpcPacketType.CLOSE: {data}"));
            return;
        }
        //if (opCode == RpcPacketType.HANDSHAKE)
        //{
        //if (string.IsNullOrEmpty(data))
        //{
        //probably close?
        //}
        //else
        //{
        //probably restart?
        //}
        //SendPacket(new { v = RPC_VERSION, client_id = clientId.Value }, RpcPacketType.HANDSHAKE);
        //happens when closing discord and artemis is open?
        //TODO: investigate
        //}

        if (data.Contains("\"evt\":\"ERROR\"")) //this looks kinda stupid ¯\_(ツ)_/¯
        {
#if DEBUG
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            await System.IO.File.WriteAllTextAsync(System.IO.Path.Combine(desktop, $"discord_error_{Environment.TickCount64}.json"), data);
#endif
            var error = JsonConvert.DeserializeObject<DiscordErrorResponse>(data, JsonSerializerSettings)!;
            HandlePendingRequest(error);
            return;
        }

        IDiscordMessage discordMessage;
        try
        {
            discordMessage = JsonConvert.DeserializeObject<IDiscordMessage>(data, JsonSerializerSettings)!;
        }
        catch (Exception exc)
        {
            Error?.Invoke(this, new DiscordRpcClientException($"Error deserializing discord message: {data}", exc));
            return;
        }

        if (discordMessage is DiscordResponse discordResponse)
        {
            HandlePendingRequest(discordResponse);
        }
        else if (discordMessage is DiscordEvent discordEvent)
        {
            HandleEvent(discordEvent);
        }
        else
        {
            Error?.Invoke(this, new DiscordRpcClientException($"Discord message was neither response nor event: {data}"));
        }
    }

    private async Task<DiscordResponse> SendRequestAsync(DiscordRequest request, int timeoutMs)
    {
        TaskCompletionSource<DiscordResponse> responseCompletionSource = new();

        //add this guid to the pending requests
        _pendingRequests.Add(request.Nonce, responseCompletionSource);

        //and send the actual request to the discord client.
        await _transport.SendPacketAsync(JsonConvert.SerializeObject(request, JsonSerializerSettings), RpcPacketType.FRAME,
            _cancellationTokenSource.Token);

        CancellationTokenSource timeoutToken = new(TimeSpan.FromMilliseconds(timeoutMs));
        timeoutToken.Token.Register(() =>
            responseCompletionSource.TrySetException(new TimeoutException($"Discord request timed out after {timeoutMs}")));

        //this will wait until the response with the expected Guid is received
        //and processed by the read loop.
        return await responseCompletionSource.Task;
    }

    private async Task<DiscordResponse<T>> SendRequestWithResponseTypeAsync<T>(DiscordRequest request, int timeoutMs = 1000) where T : class
    {
        var response = await SendRequestAsync(request, timeoutMs);

        if (response is DiscordResponse<T> typedResponse) return typedResponse;
        
        if (response is DiscordErrorResponse errorResponse)
        {
            throw new DiscordException($"Discord responded with an error: {errorResponse.Data.Message}");
        }
        
        throw new DiscordRpcClientException("Discord response was not of the specified type.", new InvalidCastException());
    }

    private void HandlePendingRequest(DiscordResponse message)
    {
        if (_pendingRequests.TryGetValue(message.Nonce, out var tcs))
        {
            if (!tcs.TrySetResult(message))
                Error?.Invoke(this, new DiscordRpcClientException("Failed to set task result. Perhaps the task timed out?"));

            _pendingRequests.Remove(message.Nonce);
        }
        else
        {
            Error?.Invoke(this, new DiscordRpcClientException("Received response with unknown guid"));
        }
    }

    private void HandleEvent(DiscordEvent discordEvent)
    {
        switch (discordEvent)
        {
            case DiscordReadyEvent ready:
                _readyTcs?.SetResult(ready.Data);
                break;
            case DiscordVoiceSettingsUpdateEvent voice:
                VoiceSettingsUpdated?.Invoke(this, voice.Data);
                break;
            case DiscordVoiceConnectionStatusEvent voiceStatus:
                VoiceConnectionStatusUpdated?.Invoke(this, voiceStatus.Data);
                break;
            case DiscordNotificationCreateEvent notif:
                NotificationReceived?.Invoke(this, notif.Data);
                break;
            case DiscordSpeakingStopEvent stop:
                SpeakingStopped?.Invoke(this, stop.Data);
                break;
            case DiscordSpeakingStartEvent start:
                SpeakingStarted?.Invoke(this, start.Data);
                break;
            case DiscordVoiceChannelSelectEvent voiceSelect:
                VoiceChannelUpdated?.Invoke(this, voiceSelect.Data);
                break;
            case DiscordVoiceStateCreateEvent voiceCreate:
                VoiceStateCreated?.Invoke(this, voiceCreate.Data);
                break;
            case DiscordVoiceStateUpdateEvent voiceUpdate:
                VoiceStateUpdated?.Invoke(this, voiceUpdate.Data);
                break;
            case DiscordVoiceStateDeleteEvent voiceDelete:
                VoiceStateDeleted?.Invoke(this, voiceDelete.Data);
                break;
            default:
                UnhandledEventReceived?.Invoke(this, discordEvent);
                break;
        }
    }

    private static DateTime GetDiscordStartTime()
    {
        var processNames = new[]
        {
            "discord",
            "discordptb",
            "discordcanary",
        };

        var processes = Process.GetProcesses().Where(p => processNames.Contains(p.ProcessName.ToLower()));

        return processes.Min(p => p.StartTime);
    }

    #region IDisposable

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                    _transport.Dispose();
                    _authClient.Dispose();

                    Error = null;
                    UnhandledEventReceived = null;
                    VoiceSettingsUpdated = null;
                    VoiceConnectionStatusUpdated = null;
                    NotificationReceived = null;
                    SpeakingStarted = null;
                    SpeakingStopped = null;
                    VoiceChannelUpdated = null;
                    VoiceStateCreated = null;
                    VoiceStateUpdated = null;
                    VoiceStateDeleted = null;

                    try
                    {
                        _readLoopTask?.Wait();
                    }
                    catch
                    {
                        /*catch everything*/
                    }

                    _cancellationTokenSource.Dispose();
                }
                catch
                {
                    //¯\_(ツ)_/¯
                }
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}