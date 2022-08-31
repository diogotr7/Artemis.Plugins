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
                DataModel.User.Username = e.User.Username;
                DataModel.User.Discriminator = e.User.Discriminator;
                DataModel.User.Id = e.User.Id;

                //Initial request for data, then use events after
                var voiceSettings = await discordClient.GetAsync<VoiceSettings>(DiscordRpcCommand.GET_VOICE_SETTINGS);

                ApplyVoiceSettings(voiceSettings);

                await UpdateCurrentVoiceChannelData();

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
            if (e.UserId == DataModel.User.Id)
            {
                DataModel.VoiceConnection.Speaking = true;
            }
            //todo
        }

        private void OnSpeakingStopped(object sender, SpeakingStartStop e)
        {
            if (e.UserId == DataModel.User.Id)
            {
                DataModel.VoiceConnection.Speaking = false;
            }
            //todo
        }

        private void OnUnhandledEventReceived(object sender, DiscordEvent discordEvent)
        {
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
            if (e.ChannelId is not null)//join voice channel
            {
                DataModel.VoiceConnection.Connected.Trigger();
                await UpdateCurrentVoiceChannelData();
            }
            else//leave voice channel
            {
                DataModel.VoiceConnection.Disconnected.Trigger();
                await UpdateCurrentVoiceChannelData();
            }
        }

        private void OnVoiceConnectionStatusUpdated(object sender, VoiceConnectionStatus e)
        {
            DataModel.VoiceConnection.State = Enum.Parse<DiscordVoiceChannelState>(e.State);
            DataModel.VoiceConnection.Ping = e.LastPing;
            DataModel.VoiceConnection.Hostname = e.Hostname;
        }

        private void OnVoiceSettingsUpdated(object sender, VoiceSettings e)
        {
            ApplyVoiceSettings(e);
        }

        private void OnVoiceStateCreated(object sender, UserVoiceState e)
        {
            //todo
        }

        private void OnVoiceStateDeleted(object sender, UserVoiceState e)
        {
            //todo
        }

        private void OnVoiceStateUpdated(object sender, UserVoiceState e)
        {
            //todo
        }
        #endregion

        private void ApplyVoiceSettings(VoiceSettings voiceData)
        {
            DataModel.VoiceSettings.AutomaticGainControl = voiceData.AutomaticGainControl;
            DataModel.VoiceSettings.EchoCancellation = voiceData.EchoCancellation;
            DataModel.VoiceSettings.NoiseSuppression = voiceData.NoiseSuppression;
            DataModel.VoiceSettings.Qos = voiceData.Qos;
            DataModel.VoiceSettings.SilenceWarning = voiceData.SilenceWarning;
            DataModel.VoiceSettings.Deafened = voiceData.Deaf;
            DataModel.VoiceSettings.Muted = voiceData.Mute;
            DataModel.VoiceSettings.Mode.Type = Enum.Parse<DiscordVoiceModeType>(voiceData.Mode.Type);
            DataModel.VoiceSettings.Mode.AutoThreshold = voiceData.Mode.AutoThreshold;
            DataModel.VoiceSettings.Mode.Threshold = voiceData.Mode.Threshold;
            DataModel.VoiceSettings.Mode.Shortcut = voiceData.Mode.Shortcut
                .Select(ds => new DiscordShortcut
                {
                    Type = (DiscordKeyType)ds.Type,
                    Code = ds.Code,
                    Name = ds.Name
                })
                .ToArray();
        }

        private async Task UpdateCurrentVoiceChannelData()
        {
            var selectedVoiceChannel = await discordClient.GetAsync<SelectedVoiceChannel>(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL);

            //we are not in voice, do nothing else.
            if (selectedVoiceChannel == null)
                return;

            //setup dynamic data model with users
            //todo

            //we are in a voice channel. subscribe to events for this channel.
            await SubscribeToVoiceChannelEvents(selectedVoiceChannel.Id);
        }

        private async Task SubscribeToVoiceChannelEvents(string id)
        {
            await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", id));
            await discordClient.SubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", id));
        }

        private async Task UnubscribeFromVoiceChannelEvents(string id)
        {
            await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_START, ("channel_id", id));
            await discordClient.UnsubscribeAsync(DiscordRpcEvent.SPEAKING_STOP, ("channel_id", id));
        }

        private bool AreClientIdAndSecretValid()
        {
            return _clientId.Value?.All(c => char.IsDigit(c)) == true && _clientSecret.Value?.Length > 0;
        }
    }
}
