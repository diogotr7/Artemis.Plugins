using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.Authentication;
using Artemis.Plugins.Modules.Discord.DataModels;
using Artemis.Plugins.Modules.Discord.DiscordPackets;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;
using Artemis.Plugins.Modules.Discord.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord;

[PluginFeature(Name = "Discord")]
public class DiscordModule : Module<DiscordDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; }

    private readonly ILogger _logger;
    private readonly PluginSetting<string> _clientId;
    private readonly PluginSetting<string> _clientSecret;
    private readonly PluginSetting<SavedToken> _savedToken;
    private readonly object _discordClientLock;

    private IDiscordRpcClient? discordClient;

    public DiscordModule(ILogger logger, PluginSettings pluginSettings)
    {
        ActivationRequirementMode = ActivationRequirementType.Any;
        ActivationRequirements = new()
        {
            new ProcessActivationRequirement("discord"),
            new ProcessActivationRequirement("discordptb"),
            new ProcessActivationRequirement("discordcanary"),
        };

        _logger = logger;

        _clientId = pluginSettings.GetSetting("DiscordClientId", string.Empty);
        _clientSecret = pluginSettings.GetSetting("DiscordClientSecret", string.Empty);
        _savedToken = pluginSettings.GetSetting<SavedToken>("DiscordToken", null);

        _discordClientLock = new();
    }

    public override void Enable()
    {
        if (!AreClientIdAndSecretValid())
        {
            _logger.Error("Discord client ID or secret invalid");
        }
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    public override void ModuleActivated(bool isOverride)
    {
        if (isOverride)
            return;

        if (!AreClientIdAndSecretValid())
        {
            _logger.Error("Discord client ID or secret invalid");
            return;
        }

        ConnectToDiscord();
    }

    public override void ModuleDeactivated(bool isOverride)
    {
        DisconnectFromDiscord();
    }

    private void ConnectToDiscord()
    {
        ArgumentException.ThrowIfNullOrEmpty(_clientId.Value, nameof(_clientId));
        ArgumentException.ThrowIfNullOrEmpty(_clientSecret.Value, nameof(_clientSecret));
        
        lock (_discordClientLock)
        {
            discordClient = new DiscordRpcClient(_clientId.Value, _clientSecret.Value, _savedToken);
            discordClient.Authenticated += OnAuthenticated;
            discordClient.Error += OnError;
            discordClient.NotificationReceived += OnNotificationReceived;
            discordClient.SpeakingStarted += OnSpeakingStarted;
            discordClient.SpeakingStopped += OnSpeakingStopped;
            discordClient.UnhandledEventReceived += OnUnhandledEventReceived;
            discordClient.VoiceChannelUpdated += OnVoiceChannelUpdated;
            discordClient.VoiceConnectionStatusUpdated += OnVoiceConnectionStatusUpdated;
            discordClient.VoiceSettingsUpdated += OnVoiceSettingsUpdated;
            discordClient.VoiceStateCreated += OnVoiceStateCreated;
            discordClient.VoiceStateDeleted += OnVoiceStateDeleted;
            discordClient.VoiceStateUpdated += OnVoiceStateUpdated;
            discordClient.Connect();
        }
    }

    private void DisconnectFromDiscord()
    {
        lock (_discordClientLock)
        {
            if (discordClient is null)
                return;
            
            discordClient.Authenticated -= OnAuthenticated;
            discordClient.Error -= OnError;
            discordClient.NotificationReceived -= OnNotificationReceived;
            discordClient.SpeakingStarted -= OnSpeakingStarted;
            discordClient.SpeakingStopped -= OnSpeakingStopped;
            discordClient.UnhandledEventReceived -= OnUnhandledEventReceived;
            discordClient.VoiceChannelUpdated -= OnVoiceChannelUpdated;
            discordClient.VoiceConnectionStatusUpdated -= OnVoiceConnectionStatusUpdated;
            discordClient.VoiceSettingsUpdated -= OnVoiceSettingsUpdated;
            discordClient.VoiceStateCreated -= OnVoiceStateCreated;
            discordClient.VoiceStateDeleted -= OnVoiceStateDeleted;
            discordClient.VoiceStateUpdated -= OnVoiceStateUpdated;
            discordClient.Dispose();
            discordClient = null;
        }
    }

    #region Event Handlers
    private async void OnAuthenticated(object? sender, Authenticate e)
    {
        if (discordClient is null)
            return;

        try
        {
            DataModel.User.Apply(e.User);

            //Initial request for data, then use events after
            var voiceSettings = await discordClient.GetAsync<VoiceSettings>(DiscordRpcCommand.GET_VOICE_SETTINGS);
            DataModel.Voice.Settings.Apply(voiceSettings);

            var selectedVoiceChannel = await discordClient.GetAsync<SelectedVoiceChannel>(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL);
            if (selectedVoiceChannel is not null)
            {
                await HandleVoiceChannelConnected(selectedVoiceChannel);
            }

            //Subscribe to these events as well
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_SETTINGS_UPDATE);
            await discordClient.SubscribeAsync(DiscordRpcEvent.NOTIFICATION_CREATE);
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_CONNECTION_STATUS);
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_CHANNEL_SELECT);
        }
        catch (Exception exc)
        {
            _logger.Error(exc, "Error during OnDiscordAuthenticated");
        }
    }

    private void OnError(object? sender, DiscordRpcClientException e)
    {
        _logger.Error(e, "Discord Rpc client error");

        if (!e.ShouldReconnect)
            return;

        //i hate this but otherwise it won't dispose properly.
        //this method is still running in the read loop thread.
        Task.Run(() =>
        {
            DisconnectFromDiscord();
        });
    }

    private void OnNotificationReceived(object? sender, Notification e)
    {
        DataModel.Notification.Trigger(new DiscordNotificationEventArgs
        {
            Body = e.Body,
            ChannelId = e.ChannelId,
            IconUrl = e.IconUrl,
            Author = e.Message.Author,
            Title = e.Title
        });
    }

    private void OnSpeakingStarted(object? sender, SpeakingStartStop e)
    {
        var member = GetMeOrOtherMember(e.UserId);
        member.IsSpeaking = true;

        UpdateIsAnyoneElseSpeaking();
    }

    private void OnSpeakingStopped(object? sender, SpeakingStartStop e)
    {
        var member = GetMeOrOtherMember(e.UserId);
        member.IsSpeaking = false;

        UpdateIsAnyoneElseSpeaking();
    }

    private void UpdateIsAnyoneElseSpeaking()
    {
        if (IsPropertyInUse(dm => dm.Voice.Channel.Members.IsAnyoneElseSpeaking, false))
        {
            DataModel.Voice.Channel.Members.IsAnyoneElseSpeaking =
                DataModel.Voice.Channel.Members.DynamicChildren.Values
                    .OfType<DynamicChild<DiscordVoiceChannelMember>>()
                    .Any(dvcm => dvcm.Value.IsSpeaking);
        }
    }

    private void OnUnhandledEventReceived(object? sender, DiscordEvent discordEvent)
    {
        if (!_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            return;

        try
        {
            _logger.Verbose("Received discord event {Event} with data {Data}", discordEvent.Event, discordEvent.GetType().GetProperty("Data")?.GetValue(discordEvent));
        }
        catch
        {
            _logger.Verbose("Received unexpected discord event of type {eventType}", discordEvent.Event);
        }
    }

    private async void OnVoiceChannelUpdated(object? sender, VoiceChannelSelect e)
    {
        if (discordClient is null)
            return;

        try
        {
            if (e.ChannelId is not null)//join voice channel
            {
                DataModel.Voice.Connected.Trigger();
                var selectedVoiceChannel = await discordClient.GetAsync<SelectedVoiceChannel>(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL);
                await HandleVoiceChannelConnected(selectedVoiceChannel);
            }
            else//leave voice channel
            {
                DataModel.Voice.Disconnected.Trigger();
                await HandleVoiceChannelDisconnected();
            }
        }
        catch (Exception exc)
        {
            _logger.Error(exc, "Exception in OnVoiceChannelUpdated");
        }
    }

    private void OnVoiceConnectionStatusUpdated(object? sender, VoiceConnectionStatus e)
    {
        DataModel.Voice.Connection.Apply(e);
    }

    private void OnVoiceSettingsUpdated(object? sender, VoiceSettings e)
    {
        DataModel.Voice.Settings.Apply(e);
    }

    private void OnVoiceStateCreated(object? sender, UserVoiceState e)
    {
        var member = GetMeOrOtherMember(e.User.Id);
        member.Apply(e);
    }

    private void OnVoiceStateDeleted(object? sender, UserVoiceState e)
    {
        if (e.User.Id == DataModel.User.Id)
        {
            DataModel.Voice.Channel.Members.Me = new();
            return;
        }
        
        DataModel.Voice.Channel.Members.RemoveDynamicChildByKey(e.User.Id);
    }

    private void OnVoiceStateUpdated(object? sender, UserVoiceState e)
    {
        var member = GetMeOrOtherMember(e.User.Id);
        member.Apply(e);
    }
    
    private DiscordVoiceChannelMember GetMeOrOtherMember(string userId)
    {
        if (userId == DataModel.User.Id)
            return DataModel.Voice.Channel.Members.Me;

        if (!DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(userId, out var member))
            member = DataModel.Voice.Channel.Members.AddDynamicChild<DiscordVoiceChannelMember>(userId, new(), string.Empty);

        return member.Value;
    }
    #endregion

    private async Task HandleVoiceChannelConnected(SelectedVoiceChannel e)
    {
        foreach (var voiceState in e.VoiceStates)
        {
            var member = GetMeOrOtherMember(voiceState.User.Id);
            member.Apply(voiceState);
        }

        DataModel.Voice.Channel.Apply(e);

        await SubscribeToVoiceChannelEvents(e.Id);
    }

    private async Task HandleVoiceChannelDisconnected()
    {
        DataModel.Voice.Channel.Members.Me = new();
        DataModel.Voice.Channel.Members.ClearDynamicChildren();

        await UnsubscribeFromVoiceChannelEvents();
    }

    private async Task SubscribeToVoiceChannelEvents(string channelId)
    {
        if (discordClient is null)
            return;

        await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", channelId));
        await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", channelId));
        await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_CREATE, ("channel_id", channelId));
        await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_UPDATE, ("channel_id", channelId));
        await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_DELETE, ("channel_id", channelId));
    }

    private Task UnsubscribeFromVoiceChannelEvents()
    {
        //todo: do we even need to do this?
        //await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", channelId));
        //await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", channelId));
        //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_CREATE, ("channel_id", channelId));
        //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_UPDATE, ("channel_id", channelId));
        //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_DELETE, ("channel_id", channelId));
        return Task.CompletedTask;
    }

    private bool AreClientIdAndSecretValid()
    {
        return _clientId.Value?.All(c => char.IsDigit(c)) == true && _clientSecret.Value?.Length > 0;
    }
}
