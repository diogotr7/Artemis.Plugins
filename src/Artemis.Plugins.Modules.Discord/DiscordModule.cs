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

        private DiscordRpcClient discordClient;

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
                discordClient = new(_clientId.Value, _clientSecret.Value, _savedToken);
                discordClient.Authenticated += OnDiscordAuthenticated;
                discordClient.EventReceived += OnDiscordEventReceived;
                discordClient.Error += OnDiscordError;
                discordClient.Connect();
            }
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            lock (_discordClientLock)
            {
                discordClient.Authenticated -= OnDiscordAuthenticated;
                discordClient.EventReceived -= OnDiscordEventReceived;
                discordClient.Error -= OnDiscordError;
                discordClient.Dispose();
            }
        }

        private void OnDiscordError(object sender, Exception e)
        {
            _logger.Error(e, "Discord Rpc client error");
        }

        private async void OnDiscordAuthenticated(object sender, Authenticate e)
        {
            try
            {
                DataModel.User.Username = e.User.Username;
                DataModel.User.Discriminator = e.User.Discriminator;
                DataModel.User.Id = e.User.Id;

                //Initial request for data, then use events after
                DiscordResponse<VoiceSettings> voiceSettingsResponse = await discordClient.SendRequestAsync<VoiceSettings>(
                        new DiscordRequest(DiscordRpcCommand.GET_VOICE_SETTINGS)
                );
                
                UpdateVoiceSettings(voiceSettingsResponse.Data);

                await UpdateVoiceChannelData();

                //Subscribe to these events as well
                await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_SETTINGS_UPDATE));
                await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.NOTIFICATION_CREATE));
                await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CONNECTION_STATUS));
                await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CHANNEL_SELECT));
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Error during OnDiscordAuthenticated");
            }
        }

        private async void OnDiscordEventReceived(object sender, DiscordEvent discordEvent)
        {
            try
            {
                switch (discordEvent)
                {
                    case DiscordEvent<VoiceSettings> voice:
                        VoiceSettings voiceData = voice.Data;
                        UpdateVoiceSettings(voiceData);
                        break;
                    case DiscordEvent<VoiceConnectionStatus> voiceStatus:
                        DataModel.VoiceConnection.State = Enum.Parse<DiscordVoiceChannelState>(voiceStatus.Data.State);
                        DataModel.VoiceConnection.Ping = voiceStatus.Data.LastPing;
                        DataModel.VoiceConnection.Hostname = voiceStatus.Data.Hostname;
                        break;
                    case DiscordEvent<Notification> notif:
                        DataModel.Notification.Trigger(new DiscordNotificationEventArgs
                        {
                            Body = notif.Data.Body,
                            ChannelId = notif.Data.ChannelId,
                            IconUrl = notif.Data.IconUrl,
                            Author = notif.Data.Message.Author,
                            Title = notif.Data.Title
                        });
                        break;
                    case DiscordEvent<SpeakingStop> speakingStop:
                        if (speakingStop.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceConnection.Speaking = false;
                        }
                        break;
                    case DiscordEvent<SpeakingStart> speakingStart:
                        if (speakingStart.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceConnection.Speaking = true;
                        }
                        break;
                    case DiscordEvent<VoiceChannelSelect> voiceSelect:
                        if (voiceSelect.Data.ChannelId is not null)//join voice channel
                        {
                            DataModel.VoiceConnection.Connected.Trigger();
                            await UpdateVoiceChannelData();
                        }
                        else//leave voice channel
                        {
                            DataModel.VoiceConnection.Disconnected.Trigger();
                        }
                        break;
                    default:
                        _logger.Error("Received unexpected discord event of type {eventType}", discordEvent.Event);
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error handling discord event.");
            }
        }

        private void UpdateVoiceSettings(VoiceSettings voiceData)
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

        private async Task UpdateVoiceChannelData()
        {
            DiscordResponse<SelectedVoiceChannel> selectedVoiceChannelResponse =
                await discordClient.SendRequestAsync<SelectedVoiceChannel>(
                    new DiscordRequest(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL));

            if (selectedVoiceChannelResponse.Data != null)
                await SubscribeToSpeakingEventsAsync(selectedVoiceChannelResponse.Data.Id);
        }

        private async Task SubscribeToSpeakingEventsAsync(string id)
        {
            await discordClient.SendRequestAsync<Subscribe>(
                new DiscordSubscribe(DiscordRpcEvent.SPEAKING_START)
                    .WithArgument("channel_id", id));
            await discordClient.SendRequestAsync<Subscribe>(
                new DiscordSubscribe(DiscordRpcEvent.SPEAKING_STOP)
                    .WithArgument("channel_id", id));
        }

        private bool AreClientIdAndSecretValid()
        {
            return _clientId.Value?.All(c => char.IsDigit(c)) == true && _clientSecret.Value?.Length > 0;
        }
    }
}
