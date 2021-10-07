using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DataModels;
using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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

        private readonly PluginSetting<string> _clientId;
        private readonly PluginSetting<string> _clientSecret;
        private readonly PluginSetting<SavedToken> _token;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        private DiscordRpcClient discordClient;

        public DiscordModule(PluginSettings pluginSettings, ILogger logger)
        {
            ActivationRequirementMode = ActivationRequirementType.Any;
            ActivationRequirements = new()
            {
                new ProcessActivationRequirement("discord"),
                new ProcessActivationRequirement("discordptb"),
                new ProcessActivationRequirement("discordcanary"),
            };

            _clientId = pluginSettings.GetSetting<string>("DiscordClientId", null);
            _clientSecret = pluginSettings.GetSetting<string>("DiscordClientSecret", null);
            _token = pluginSettings.GetSetting<SavedToken>("DiscordToken", null);
            _logger = logger;
            _httpClient = new();
        }

        public override void Enable()
        {
            if (_clientId.Value == null || _clientId.Value.Length != 18 ||
                _clientSecret.Value == null || _clientSecret.Value.Length != 32)
                throw new ArtemisPluginException("Client ID or secret invalid");

            AddTimedUpdate(TimeSpan.FromDays(1), TryRefreshTokenAsync);
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

            discordClient = new(_clientId.Value);
            discordClient.EventReceived += OnDiscordEventReceived;
            discordClient.Error += OnDiscordError;
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            if (isOverride)
                return;

            discordClient?.Dispose();
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
                    case DiscordEvent<ReadyData>:
                        if (_token.Value == null)
                        {
                            //We have no token saved. This means it's probably the first time
                            //the user is using the plugin. We need to ask for their permission
                            //to get a token from discord. 
                            //This token can be saved and reused (+ refreshed) later.
                            var authorizeResponse = await discordClient.SendRequestAsync<AuthorizeData>(
                                new DiscordRequest(DiscordRpcCommand.AUTHORIZE)
                                    .WithArgument("client_id", _clientId.Value)
                                    .WithArgument("scopes", SCOPES),
                                timeoutMs: 15000);//high timeout so the user has time to click the button

                            SaveToken(await GetAccessTokenAsync(authorizeResponse.Data.Code));
                        }
                        else
                        {
                            //If we already have a token saved from earlier,
                            //we need to check if it expired or not.
                            //If yes, refresh it.
                            //Then, authenticate.
                            if (_token.Value.ExpirationDate < DateTime.UtcNow)
                            {
                                SaveToken(await RefreshAccessTokenAsync(_token.Value.RefreshToken));
                            }
                        }

                        var authenticateResponse =
                            await discordClient.SendRequestAsync<AuthenticateData>(
                                new DiscordRequest(DiscordRpcCommand.AUTHENTICATE)
                                    .WithArgument("access_token", _token.Value.AccessToken));

                        DataModel.User.Username = authenticateResponse.Data.User.Username;
                        DataModel.User.Discriminator = authenticateResponse.Data.User.Discriminator;
                        DataModel.User.Id = authenticateResponse.Data.User.Id;

                        //Initial request for data, then use events after
                        var voiceSettingsResponse =
                            await discordClient.SendRequestAsync<VoiceSettingsData>(
                                new DiscordRequest(DiscordRpcCommand.GET_VOICE_SETTINGS));

                        DataModel.VoiceSettings.Deafened = voiceSettingsResponse.Data.Deaf;
                        DataModel.VoiceSettings.Muted = voiceSettingsResponse.Data.Mute;

                        await UpdateVoiceChannelData();

                        //Subscribe to these events as well
                        await discordClient.SendRequestAsync<SubscribeData>(new DiscordSubscribe(DiscordRpcEvent.VOICE_SETTINGS_UPDATE));
                        await discordClient.SendRequestAsync<SubscribeData>(new DiscordSubscribe(DiscordRpcEvent.NOTIFICATION_CREATE));
                        await discordClient.SendRequestAsync<SubscribeData>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CONNECTION_STATUS));
                        await discordClient.SendRequestAsync<SubscribeData>(new DiscordSubscribe(DiscordRpcEvent.VOICE_CHANNEL_SELECT));
                        break;
                    case DiscordEvent<VoiceSettingsData> voice:
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
                    case DiscordEvent<VoiceConnectionStatusData> voiceStatus:
                        DataModel.VoiceConnection.State = voiceStatus.Data.State;
                        DataModel.VoiceConnection.Ping = voiceStatus.Data.LastPing;
                        DataModel.VoiceConnection.Hostname = voiceStatus.Data.Hostname;
                        break;
                    case DiscordEvent<NotificationCreateData> notif:
                        DataModel.Notification.Trigger(new DiscordNotificationEventArgs
                        {
                            Body = notif.Data.Body,
                            ChannelId = notif.Data.ChannelId,
                            IconUrl = notif.Data.IconUrl,
                            Author = notif.Data.Message.Author,
                            Title = notif.Data.Title
                        });
                        break;
                    case DiscordEvent<SpeakingStopData> speakingStop:
                        if (speakingStop.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceConnection.Speaking = false;
                        }
                        break;
                    case DiscordEvent<SpeakingStartData> speakingStart:
                        if (speakingStart.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceConnection.Speaking = true;
                        }
                        break;
                    case DiscordEvent<VoiceChannelSelectData> voiceSelect:
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
                await discordClient.SendRequestAsync<SelectedVoiceChannelData>(
                    new DiscordRequest(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL));

            if (selectedVoiceChannelResponse.Data != null)
                await SubscribeToSpeakingEventsAsync(selectedVoiceChannelResponse.Data.Id);
        }

        private async Task SubscribeToSpeakingEventsAsync(string id)
        {
            await discordClient.SendRequestAsync<SubscribeData>(
                new DiscordSubscribe(DiscordRpcEvent.SPEAKING_START)
                .WithArgument("channel_id", id));
            await discordClient.SendRequestAsync<SubscribeData>(
                new DiscordSubscribe(DiscordRpcEvent.SPEAKING_STOP)
                .WithArgument("channel_id", id));
        }

        #region Authorization & Authentication
        private void SaveToken(TokenResponse newToken)
        {
            _token.Value = new SavedToken
            {
                AccessToken = newToken.AccessToken,
                RefreshToken = newToken.RefreshToken,
                ExpirationDate = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn)
            };
            _token.Save();
        }

        private async Task TryRefreshTokenAsync(double obj)
        {
            if (_token.Value == null)
                return;

            if (_token.Value.ExpirationDate >= DateTime.UtcNow.AddDays(1))
                return;

            try
            {
                SaveToken(await RefreshAccessTokenAsync(_token.Value.RefreshToken));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to refresh discord token.");
            }
        }

        private async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
        {
            return await GetCredentialsAsync("authorization_code", "code", challengeCode);
        }

        private async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            return await GetCredentialsAsync("refresh_token", "refresh_token", refreshToken);
        }

        private async Task<TokenResponse> GetCredentialsAsync(string grantType, string secretType, string secret)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                ["grant_type"] = grantType,
                [secretType] = secret,
                ["client_id"] = _clientId.Value,
                ["client_secret"] = _clientSecret.Value
            };

            using HttpResponseMessage response = await _httpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
            string responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Discord credentials error. Grant Type: {grantType}, Secret Type: {secretType}, Message: {message}", grantType, secretType, responseString);
                throw new UnauthorizedAccessException(responseString);
            }

            return JsonConvert.DeserializeObject<TokenResponse>(responseString);
        }
        #endregion
    }
}