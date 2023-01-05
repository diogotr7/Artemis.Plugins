using Artemis.Core;
using Artemis.Plugins.Modules.Discord.Authentication;
using Artemis.Plugins.Modules.Discord.DiscordPackets;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;
using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord;

public class DiscordRpcClient : IDiscordRpcClient
{
    #region RPC Constants
    private const string RPC_VERSION = "1";
    private const int HEADER_SIZE = 8;
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
        }
    };
    #endregion

    #region Fields
    private readonly Dictionary<Guid, TaskCompletionSource<DiscordResponse>> _pendingRequests;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly string _clientId;
    private readonly DiscordAuthClient _authClient;
    private readonly byte[] _headerBuffer;
    private NamedPipeClientStream? _pipe;
    private Task? _readLoopTask;
    private TaskCompletionSource<Ready>? _readyTcs;
    #endregion

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

    public DiscordRpcClient(string clientId, string clientSecret, PluginSetting<SavedToken> tokenSetting)
    {
        _clientId = clientId;
        _authClient = new(clientId, clientSecret, tokenSetting);
        _pendingRequests = new();
        _cancellationTokenSource = new();
        _headerBuffer = new byte[HEADER_SIZE];
    }

    public void Connect(int timeoutMs = 500)
    {
        if (_pipe?.IsConnected == true)
            throw new InvalidOperationException("Already connected");

        //we want discord to be open for at least 10 seconds 
        //when we connect. if it hasn't, sleep until then.
        var startTime = GetDiscordStartTime();
        var now = DateTime.Now;
        var desiredTime = startTime + TimeSpan.FromSeconds(10);
        if (now < desiredTime)
            Thread.Sleep(desiredTime - now);

        const int MAX_TRIES = 10;
        for (int i = 0; i < MAX_TRIES; i++)
        {
            try
            {
                _pipe = new(".", GetPipeName(i), PipeDirection.InOut, PipeOptions.Asynchronous);
                _pipe.Connect(timeoutMs);
                _readLoopTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);
                Task.Run(InitializeAsync, _cancellationTokenSource.Token);
                return;
            }
            catch (Exception e)
            {
                Error?.Invoke(this, new DiscordRpcClientException("Error connecting to discord", e));
            }
        }

        throw new DiscordRpcClientException("Failed to connect to Discord");
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

        DiscordResponse<T> response = await SendRequestWithResponseTypeAsync<T>(request);

        return response.Data;
    }

    private async Task HandshakeAsync()
    {
        string handshake = JsonConvert.SerializeObject(new { v = RPC_VERSION, client_id = _clientId }, _jsonSerializerSettings);

        await SendPacketAsync(handshake, RpcPacketType.HANDSHAKE);
    }

    private async Task InitializeAsync()
    {
        _readyTcs = new();

        await HandshakeAsync();

        //this task will complete once the Ready event is received
        await _readyTcs.Task;

        var authenticatedData = await HandleAuthenticationAsync();

        Authenticated?.Invoke(this, authenticatedData);
    }

    private async Task<Authenticate> HandleAuthenticationAsync()
    {
        if (!_authClient.HasToken)
        {
            //We have no token saved. This means it's probably the first time
            //the user is using the plugin. We need to ask for their permission
            //to get a token from discord. 
            //This token can be saved and reused (+ refreshed) later.
            DiscordResponse<Authorize> authorizeResponse = await SendRequestWithResponseTypeAsync<Authorize>(
                new DiscordRequest(DiscordRpcCommand.AUTHORIZE,
                    ("client_id", _clientId),
                    ("scopes", new string[] { "rpc", "identify", "rpc.notifications.read" })),
                timeoutMs: 30000);//high timeout so the user has time to click the button

            await _authClient.GetAccessTokenAsync(authorizeResponse.Data.Code);
        }
        if (!_authClient.IsTokenValid)
        {
            //Now that we have a token for sure,
            //we need to check if it expired or not.
            //If yes, refresh it.
            //Then, authenticate.
            await _authClient.RefreshAccessTokenAsync();
        }

        DiscordResponse<Authenticate> authenticateResponse = await SendRequestWithResponseTypeAsync<Authenticate>(
                new DiscordRequest(DiscordRpcCommand.AUTHENTICATE,
                    ("access_token", _authClient.AccessToken))
        );

        return authenticateResponse.Data;
    }

    private async Task ReadLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _pipe?.IsConnected == true)
        {
            try
            {
                var (opCode, data) = await ReadMessageAsync();

                await ProcessMessageAsync(opCode, data);

                //this is not really this classes job but it's easier to put it here :S
                //It should only execute once a week or so, which is how long the token
                //lasts for. This is to keep the connection working for the people that 
                //never close the programs.
                await _authClient.TryRefreshTokenAsync();
            }
            catch (Exception e)
            {
                Error?.Invoke(this, new DiscordRpcClientException("Error in Discord read loop", e, true));
            }
        }
    }

    private async Task<(RpcPacketType, string)> ReadMessageAsync()
    {
        byte[]? dataBuffer = null;
        try
        {
            int headerReadBytes = await _pipe.ReadAsync(_headerBuffer.AsMemory(0, HEADER_SIZE), _cancellationTokenSource.Token);

            if (headerReadBytes < HEADER_SIZE)
                throw new DiscordRpcClientException("Read less than 4 bytes for the header");

            var header = MemoryMarshal.AsRef<DiscordRpcHeader>(_headerBuffer);

            if (header.PacketLength == 0)
                throw new DiscordRpcClientException("Read zero bytes from the pipe");

            dataBuffer = ArrayPool<byte>.Shared.Rent(header.PacketLength);

            await _pipe.ReadAsync(dataBuffer.AsMemory(0, header.PacketLength), _cancellationTokenSource.Token);

            return (header.PacketType, Encoding.UTF8.GetString(dataBuffer.AsSpan(0, header.PacketLength)));
        }
        finally
        {
            if (dataBuffer != null)
                ArrayPool<byte>.Shared.Return(dataBuffer);
        }
    }

    private async Task ProcessMessageAsync(RpcPacketType opCode, string data)
    {
        if (opCode == RpcPacketType.PING)
        {
            await SendPacketAsync(data, RpcPacketType.PONG);
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

        if (data.Contains("\"evt\":\"ERROR\""))//this looks kinda stupid ¯\_(ツ)_/¯
            throw new DiscordRpcClientException($"Discord response contained an error: {data}");

        IDiscordMessage discordMessage;
        try
        {
            discordMessage = JsonConvert.DeserializeObject<IDiscordMessage>(data, _jsonSerializerSettings);
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

    private async Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType)
    {
        int stringByteLength = Encoding.UTF8.GetByteCount(stringData);
        int bufferSize = HEADER_SIZE + stringByteLength;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            if (!BitConverter.TryWriteBytes(buffer.AsSpan(0, 4), (int)rpcPacketType))
                throw new DiscordRpcClientException("Error writing rpc packet type.");

            if (!BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), stringByteLength))
                throw new DiscordRpcClientException("Error writing string byte length.");

            if (Encoding.UTF8.GetBytes(stringData, 0, stringData.Length, buffer, HEADER_SIZE) != stringData.Length)
                throw new DiscordRpcClientException("Wrote wrong number of characters.");

            await _pipe.WriteAsync(buffer.AsMemory(0, bufferSize), _cancellationTokenSource.Token);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task<DiscordResponse> SendRequestAsync(DiscordRequest request, int timeoutMs)
    {
        TaskCompletionSource<DiscordResponse> responseCompletionSource = new();

        //add this guid to the pending requests
        _pendingRequests.Add(request.Nonce, responseCompletionSource);

        //and send the actual request to the discord client.
        await SendPacketAsync(JsonConvert.SerializeObject(request, _jsonSerializerSettings), RpcPacketType.FRAME);

        CancellationTokenSource timeoutToken = new(TimeSpan.FromMilliseconds(timeoutMs));
        timeoutToken.Token.Register(() => responseCompletionSource.TrySetException(new TimeoutException($"Discord request timed out after {timeoutMs}")));

        //this will wait until the response with the expected Guid is received
        //and processed by the read loop.
        return await responseCompletionSource.Task;
    }

    private async Task<DiscordResponse<T>> SendRequestWithResponseTypeAsync<T>(DiscordRequest request, int timeoutMs = 1000) where T : class
    {
        DiscordResponse response = await SendRequestAsync(request, timeoutMs);

        if (response is not DiscordResponse<T> typedResponse)
            throw new DiscordRpcClientException("Discord response was not of the specified type.", new InvalidCastException());

        return typedResponse;
    }

    private void HandlePendingRequest(DiscordResponse message)
    {
        if (_pendingRequests.TryGetValue(message.Nonce, out TaskCompletionSource<DiscordResponse>? tcs))
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
            case DiscordEvent<Ready> ready:
                _readyTcs?.SetResult(ready.Data);
                break;
            case DiscordEvent<VoiceSettings> voice:
                VoiceSettingsUpdated?.Invoke(this, voice.Data);
                break;
            case DiscordEvent<VoiceConnectionStatus> voiceStatus:
                VoiceConnectionStatusUpdated?.Invoke(this, voiceStatus.Data);
                break;
            case DiscordEvent<Notification> notif:
                NotificationReceived?.Invoke(this, notif.Data);
                break;
            case DiscordEvent<SpeakingStartStop> stop when stop.Event == DiscordRpcEvent.SPEAKING_STOP:
                SpeakingStopped?.Invoke(this, stop.Data);
                break;
            case DiscordEvent<SpeakingStartStop> start when start.Event == DiscordRpcEvent.SPEAKING_START:
                SpeakingStarted?.Invoke(this, start.Data);
                break;
            case DiscordEvent<VoiceChannelSelect> voiceSelect:
                VoiceChannelUpdated?.Invoke(this, voiceSelect.Data);
                break;
            case DiscordEvent<UserVoiceState> voiceCreate when voiceCreate.Event == DiscordRpcEvent.VOICE_STATE_CREATE:
                VoiceStateCreated?.Invoke(this, voiceCreate.Data);
                break;
            case DiscordEvent<UserVoiceState> voiceUpdate when voiceUpdate.Event == DiscordRpcEvent.VOICE_STATE_UPDATE:
                VoiceStateUpdated?.Invoke(this, voiceUpdate.Data);
                break;
            case DiscordEvent<UserVoiceState> voiceDelete when voiceDelete.Event == DiscordRpcEvent.VOICE_STATE_DELETE:
                VoiceStateDeleted?.Invoke(this, voiceDelete.Data);
                break;
            default:
                UnhandledEventReceived?.Invoke(this, discordEvent);
                break;
        }
    }

    #region Helper Pipe Methods
    private static string GetPipeName(int index)
    {
        const string PIPE_NAME = "discord-ipc-{0}";
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => Path.Combine(GetTemporaryDirectory(), string.Format(PIPE_NAME, index)),
            _ => string.Format(PIPE_NAME, index)
        };
    }

    private static string GetTemporaryDirectory()
    {
        //source: https://github.com/Lachee/discord-rpc-csharp/
        //try all these possible paths it could be, depending on system configuration
        return Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ??
            Environment.GetEnvironmentVariable("TMPDIR") ??
            Environment.GetEnvironmentVariable("TMP") ??
            Environment.GetEnvironmentVariable("TEMP") ??
            "/tmp";
    }
    #endregion

    private DateTime GetDiscordStartTime()
    {
        var processNames = new string[]
        {
            "discord",
            "discordptb",
            "discordcanary",
        };

        var processes = Process.GetProcesses().Where(p => processNames.Contains(p.ProcessName.ToLower()));

        return processes.Min(p => p.StartTime);
    }

    #region IDisposable
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                    _pipe?.Dispose();
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

                    try { _readLoopTask?.Wait(); }
                    catch { /*catch everything*/ }

                    _cancellationTokenSource.Dispose();
                }
                catch
                {
                    //¯\_(ツ)_/¯
                }
            }

            disposedValue = true;
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
