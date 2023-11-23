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
    private readonly PluginSettings _pluginSettings;
    private readonly object _discordClientLock;

    private IDiscordRpcClient? _discordClient;

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
        _pluginSettings = pluginSettings;
        _discordClientLock = new object();
    }

    public override void Enable()
    {
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

        ConnectToDiscord();
    }

    public override void ModuleDeactivated(bool isOverride)
    {
        DisconnectFromDiscord();
    }

    internal void ConnectToDiscord()
    {
        lock (_discordClientLock)
        {
            _discordClient = new DiscordRpcClient(_pluginSettings);
            _discordClient.Authenticated += OnAuthenticated;
            _discordClient.Error += OnError;
            _discordClient.NotificationReceived += OnNotificationReceived;
            _discordClient.SpeakingStarted += OnSpeakingStarted;
            _discordClient.SpeakingStopped += OnSpeakingStopped;
            _discordClient.UnhandledEventReceived += OnUnhandledEventReceived;
            _discordClient.VoiceChannelUpdated += OnVoiceChannelUpdated;
            _discordClient.VoiceConnectionStatusUpdated += OnVoiceConnectionStatusUpdated;
            _discordClient.VoiceSettingsUpdated += OnVoiceSettingsUpdated;
            _discordClient.VoiceStateCreated += OnVoiceStateCreated;
            _discordClient.VoiceStateDeleted += OnVoiceStateDeleted;
            _discordClient.VoiceStateUpdated += OnVoiceStateUpdated;
            _discordClient.Connect().Wait();
        }
    }

    internal void DisconnectFromDiscord()
    {
        lock (_discordClientLock)
        {
            if (_discordClient is null)
                return;
            
            _discordClient.Authenticated -= OnAuthenticated;
            _discordClient.Error -= OnError;
            _discordClient.NotificationReceived -= OnNotificationReceived;
            _discordClient.SpeakingStarted -= OnSpeakingStarted;
            _discordClient.SpeakingStopped -= OnSpeakingStopped;
            _discordClient.UnhandledEventReceived -= OnUnhandledEventReceived;
            _discordClient.VoiceChannelUpdated -= OnVoiceChannelUpdated;
            _discordClient.VoiceConnectionStatusUpdated -= OnVoiceConnectionStatusUpdated;
            _discordClient.VoiceSettingsUpdated -= OnVoiceSettingsUpdated;
            _discordClient.VoiceStateCreated -= OnVoiceStateCreated;
            _discordClient.VoiceStateDeleted -= OnVoiceStateDeleted;
            _discordClient.VoiceStateUpdated -= OnVoiceStateUpdated;
            _discordClient.Dispose();
            _discordClient = null;
        }
    }

    #region Event Handlers
    private async void OnAuthenticated(object? sender, Authenticate e)
    {
        if (_discordClient is null)
            return;

        try
        {
            DataModel.User.Apply(e.User);

            //Initial request for data, then use events after
            var voiceSettings = await _discordClient.GetAsync<VoiceSettings>(DiscordRpcCommand.GET_VOICE_SETTINGS);
            DataModel.Voice.Settings.Apply(voiceSettings);

            var selectedVoiceChannel = await _discordClient.GetAsync<SelectedVoiceChannel>(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL);
            if (selectedVoiceChannel is not null)
            {
                await HandleVoiceChannelConnected(selectedVoiceChannel);
            }

            //Subscribe to these events as well
            await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_SETTINGS_UPDATE);
            await _discordClient.SubscribeAsync(DiscordRpcEvent.NOTIFICATION_CREATE);
            await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_CONNECTION_STATUS);
            await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_CHANNEL_SELECT);
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
        if (IsPropertyInUse(dm => dm.Voice.Channel.IsAnyoneElseSpeaking, false))
        {
            DataModel.Voice.Channel.IsAnyoneElseSpeaking =
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
        if (_discordClient is null)
            return;

        try
        {
            if (e.ChannelId is not null)//join voice channel
            {
                DataModel.Voice.Connected.Trigger();
                var selectedVoiceChannel = await _discordClient.GetAsync<SelectedVoiceChannel>(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL);
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
        var member = GetMeOrOtherMember(e.User.Id, e.Nick);
        member.Apply(e);
    }

    private void OnVoiceStateDeleted(object? sender, UserVoiceState e)
    {
        if (e.User.Id == DataModel.User.Id)
        {
            DataModel.Voice.Channel.Me.Reset();
            return;
        }
        
        DataModel.Voice.Channel.Members.RemoveDynamicChildByKey(e.User.Id);
    }

    private void OnVoiceStateUpdated(object? sender, UserVoiceState e)
    {
        var member = GetMeOrOtherMember(e.User.Id, e.Nick);
        member.Apply(e);
    }
    
    private DiscordVoiceChannelMember GetMeOrOtherMember(string userId, string nickname = "")
    {
        if (userId == DataModel.User.Id)
            return DataModel.Voice.Channel.Me;

        //if we get here from SpeakingStarted/Stopped, the user should already be in the list.
        //If it's not, the nickname will be empty sadly. Functionality is still there but it's not as nice :(
        if (!DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(userId, out var member))
            member = DataModel.Voice.Channel.Members.AddDynamicChild<DiscordVoiceChannelMember>(userId, new(), nickname);

        return member.Value;
    }
    #endregion

    private async Task HandleVoiceChannelConnected(SelectedVoiceChannel e)
    {
        foreach (var voiceState in e.VoiceStates)
        {
            var member = GetMeOrOtherMember(voiceState.User.Id, voiceState.Nick);
            member.Apply(voiceState);
        }

        DataModel.Voice.Channel.Apply(e);

        await SubscribeToVoiceChannelEvents(e.Id);
    }

    private async Task HandleVoiceChannelDisconnected()
    {
        DataModel.Voice.Channel.Reset();

        await UnsubscribeFromVoiceChannelEvents();
    }

    private async Task SubscribeToVoiceChannelEvents(string channelId)
    {
        if (_discordClient is null)
            return;

        await _discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", channelId));
        await _discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", channelId));
        await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_CREATE, ("channel_id", channelId));
        await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_UPDATE, ("channel_id", channelId));
        await _discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_DELETE, ("channel_id", channelId));
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
}
