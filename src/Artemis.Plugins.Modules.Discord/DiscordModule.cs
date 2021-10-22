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
        private static readonly string[] SCOPES = new string[]
        {
            "rpc",
            "identify",
            "rpc.notifications.read"
        };

        public override List<IModuleActivationRequirement> ActivationRequirements { get; }

        private readonly ILogger _logger;
        private readonly PluginSetting<string> _clientId;
        private readonly PluginSetting<string> _clientSecret;
        private readonly DiscordAuthClient _authClient;
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

            _clientId = pluginSettings.GetSetting<string>("DiscordClientId", null);
            _clientSecret = pluginSettings.GetSetting<string>("DiscordClientSecret", null);
            var tokenSetting = pluginSettings.GetSetting<SavedToken>("DiscordToken", null);

            _authClient = new(_clientId, _clientSecret, tokenSetting);

            _discordClientLock = new();
        }

        public override void Enable()
        {
            if (_clientId.Value == null || _clientId.Value.Length != 18 ||
                _clientSecret.Value == null || _clientSecret.Value.Length != 32)
                throw new ArtemisPluginException("Client ID or secret invalid");

            AddTimedUpdate(TimeSpan.FromDays(1), (_) => _authClient.TryRefreshTokenAsync());
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
            lock (_discordClientLock)
            {
                discordClient = new(_clientId.Value);
                discordClient.EventReceived += OnDiscordEventReceived;
                discordClient.Error += OnDiscordError;
                discordClient.Connect();
            }
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            lock (_discordClientLock)
            {
                discordClient.EventReceived -= OnDiscordEventReceived;
                discordClient.Error -= OnDiscordError;
                discordClient.Dispose();
            }
        }

        private void OnDiscordError(object sender, Exception e)
        {
            _logger.Error(e, "Discord Rpc client error");
        }

        private async void OnDiscordEventReceived(object sender, DiscordEvent discordEvent)
        {
            try
            {
                switch (discordEvent)
                {
                    case DiscordEvent<Ready>:
                        if (!_authClient.HasToken)
                        {
                            //We have no token saved. This means it's probably the first time
                            //the user is using the plugin. We need to ask for their permission
                            //to get a token from discord. 
                            //This token can be saved and reused (+ refreshed) later.
                            var authorizeResponse = await discordClient.SendRequestAsync<Authorize>(
                                new DiscordRequest(DiscordRpcCommand.AUTHORIZE)
                                    .WithArgument("client_id", _clientId.Value)
                                    .WithArgument("scopes", SCOPES),
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

                        var authenticateResponse = await discordClient.SendRequestAsync<Authenticate>(
                                new DiscordRequest(DiscordRpcCommand.AUTHENTICATE)
                                    .WithArgument("access_token", _authClient.AccessToken)
                        );

                        DataModel.User.Username = authenticateResponse.Data.User.Username;
                        DataModel.User.Discriminator = authenticateResponse.Data.User.Discriminator;
                        DataModel.User.Id = authenticateResponse.Data.User.Id;

                        //Initial request for data, then use events after
                        var voiceSettingsResponse = await discordClient.SendRequestAsync<VoiceSettings>(
                                new DiscordRequest(DiscordRpcCommand.GET_VOICE_SETTINGS)
                        );

                        DataModel.VoiceSettings.Deafened = voiceSettingsResponse.Data.Deaf;
                        DataModel.VoiceSettings.Muted = voiceSettingsResponse.Data.Mute;

                        await UpdateVoiceChannelData();

                        //Subscribe to these events as well
                        await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_SETTINGS_UPDATE));
                        await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.NOTIFICATION_CREATE));
                        await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CONNECTION_STATUS));
                        await discordClient.SendRequestAsync<Subscribe>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CHANNEL_SELECT));
                        break;
                    case DiscordEvent<VoiceSettings> voice:
                        var voiceData = voice.Data;
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

                        break;
                    case DiscordEvent<VoiceConnectionStatus> voiceStatus:
                        DataModel.VoiceConnection.State = voiceStatus.Data.State;
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

        private async Task UpdateVoiceChannelData()
        {
            var selectedVoiceChannelResponse =
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
    }
}