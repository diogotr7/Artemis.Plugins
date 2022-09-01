using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.Authentication;
using Artemis.Plugins.Modules.Discord.DataModels;
using Artemis.Plugins.Modules.Discord.Enums;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord
{
    [PluginFeature(Name = "Discord", Icon = "Discord")]
    public class DiscordModule : Module<DiscordDataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; }

        private readonly ILogger _logger;
        private readonly PluginSetting<string> _clientId;
        private readonly PluginSetting<string> _clientSecret;
        private readonly PluginSetting<SavedToken> _savedToken;
        private readonly object _discordClientLock;

        private IDiscordRpcClient discordClient;

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

        public override void ModuleDeactivated(bool isOverride)
        {
            lock (_discordClientLock)
            {
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
            }
        }

        #region Event Handlers
        private async void OnAuthenticated(object sender, Authenticate e)
        {
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

        private void OnError(object sender, Exception e)
        {
            _logger.Error(e, "Discord Rpc client error");
        }

        private void OnNotificationReceived(object sender, Notification e)
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

        private void OnSpeakingStarted(object sender, SpeakingStartStop e)
        {
            if (DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(e.UserId, out var member))
            {
                member.Value.IsSpeaking = true;
            }
        }

        private void OnSpeakingStopped(object sender, SpeakingStartStop e)
        {
            if (DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(e.UserId, out var member))
            {
                member.Value.IsSpeaking = false;
            }
        }

        private void OnUnhandledEventReceived(object sender, DiscordEvent discordEvent)
        {
            if (!_logger.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                return;

            try
            {
                _logger.Verbose("Received discord event {Event} with data {Data}", discordEvent.Event, discordEvent.GetType().GetProperty("Data").GetValue(discordEvent));
            }
            catch
            {
                _logger.Verbose("Received unexpected discord event of type {eventType}", discordEvent.Event);
            }
        }

        private async void OnVoiceChannelUpdated(object sender, VoiceChannelSelect e)
        {
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

        private void OnVoiceConnectionStatusUpdated(object sender, VoiceConnectionStatus e)
        {
            DataModel.Voice.Connection.Apply(e);
        }

        private void OnVoiceSettingsUpdated(object sender, VoiceSettings e)
        {
            DataModel.Voice.Settings.Apply(e);
        }

        private void OnVoiceStateCreated(object sender, UserVoiceState e)
        {
            if (!DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(e.User.Id, out var member))
                member = DataModel.Voice.Channel.Members.AddDynamicChild<DiscordVoiceChannelMember>(e.User.Id, new(), e.Nick);

            member.Value.Apply(e);
        }

        private void OnVoiceStateDeleted(object sender, UserVoiceState e)
        {
            DataModel.Voice.Channel.Members.RemoveDynamicChildByKey(e.User.Id);
        }

        private void OnVoiceStateUpdated(object sender, UserVoiceState e)
        {
            if (DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(e.User.Id, out var member))
            {
                member.Value.Apply(e);
            }
        }
        #endregion

        private async Task HandleVoiceChannelConnected(SelectedVoiceChannel e)
        {
            foreach(var voiceState in e.VoiceStates)
            {
                if (!DataModel.Voice.Channel.Members.TryGetDynamicChild<DiscordVoiceChannelMember>(voiceState.User.Id, out var member))
                    member = DataModel.Voice.Channel.Members.AddDynamicChild<DiscordVoiceChannelMember>(voiceState.User.Id, new(), voiceState.Nick);

                member.Value.Apply(voiceState);
            }

            DataModel.Voice.Channel.Apply(e);

            await SubscribeToVoiceChannelEvents(e.Id);
        }

        private async Task HandleVoiceChannelDisconnected()
        {
            DataModel.Voice.Channel.Members.ClearDynamicChildren();

            await UnubscribeFromVoiceChannelEvents();
        }

        private async Task SubscribeToVoiceChannelEvents(string channelId)
        {
            await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", channelId));
            await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", channelId));
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_CREATE, ("channel_id", channelId));
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_UPDATE, ("channel_id", channelId));
            await discordClient.SubscribeAsync(DiscordRpcEvent.VOICE_STATE_DELETE, ("channel_id", channelId));
        }

        private async Task UnubscribeFromVoiceChannelEvents()
        {
            //toto: do we even need to do this?
            //await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", channelId));
            //await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", channelId));
            //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_CREATE, ("channel_id", channelId));
            //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_UPDATE, ("channel_id", channelId));
            //await discordClient.UnsubscribeAsync(DiscordRpcEvent.VOICE_STATE_DELETE, ("channel_id", channelId));
        }

        private bool AreClientIdAndSecretValid()
        {
            return _clientId.Value?.All(c => char.IsDigit(c)) == true && _clientSecret.Value?.Length > 0;
        }
    }
}
