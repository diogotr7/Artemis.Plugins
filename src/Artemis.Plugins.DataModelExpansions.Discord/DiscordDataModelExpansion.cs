using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.Discord.DataModels;
using Artemis.Plugins.DataModelExpansions.Discord.Enums;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public partial class DiscordDataModelExpansion : DataModelExpansion<DiscordDataModel>
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
            }
        };
        private static readonly HttpClient client = new HttpClient();

        private readonly PluginSetting<string> clientId;
        private readonly PluginSetting<string> clientSecret;
        private readonly PluginSetting<SavedToken> token;
        private readonly ILogger _logger;

        private static readonly string[] _scopes = new string[]
        {
            "rpc",
            "identify",
            "messages.read",
            "rpc.notifications.read"
        };
        const string PIPE = @"discord-ipc-0";
        const string RPC_VERSION = "1";
        private NamedPipeClientStream _pipe;
        private CancellationTokenSource _cancellationToken;

        public DiscordDataModelExpansion(PluginSettings pluginSettings, ILogger logger)
        {
            clientId = pluginSettings.GetSetting<string>("DiscordClientId", null);
            clientSecret = pluginSettings.GetSetting<string>("DiscordClientSecret", null);
            token = pluginSettings.GetSetting<SavedToken>("DiscordToken", null);

            //REMOVE THIS
            if (clientId.Value == null)
            {
                clientId.Value = "YOUR_CLIENT_ID";
                clientId.Save();
            }
            if (clientSecret == null)
            {
                clientSecret.Value = "YOUR_CLIENT_SECRET";
                clientSecret.Save();
            }
            //REMOVE THIS

            _logger = logger;
        }

        public override void Enable()
        {
            if (clientId.Value == null || clientSecret == null)
                throw new ArtemisPluginException("Client ID or secret invalid");

            try
            {
                ConnectIPC();
            }
            catch(Exception e)
            {
                throw new ArtemisPluginException("Failed to connect to Discord RPC", e);
            }
            
            SendPacketIPC(new { v = RPC_VERSION, client_id = clientId.Value }, RpcPacketType.HANDSHAKE);
            _cancellationToken = new CancellationTokenSource();
            Task.Run(StartReadIPC);
        }

        public override void Disable()
        {
            _cancellationToken.Cancel();
            _pipe.Dispose();
        }

        public override void Update(double deltaTime)
        {
            //nothing
        }

        #region IPC
        private void ConnectIPC()
        {
            _pipe = new NamedPipeClientStream(".", PIPE, PipeDirection.InOut, PipeOptions.None);
            _pipe.Connect(500);
        }

        private void SendPacketIPC(object obj, RpcPacketType opcode = RpcPacketType.FRAME)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _jsonSerializerSettings));
            int dataLength = data.Length;
            var sendBuff = new byte[dataLength + 8];
            var writer = new BinaryWriter(new MemoryStream(sendBuff));
            writer.Write((int)opcode);
            writer.Write(dataLength);
            writer.Write(data);
            _pipe.Write(sendBuff);
        }

        private void StartReadIPC()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var buffer = new byte[8192];
                _pipe.Read(buffer, 0, buffer.Length);
                var reader = new BinaryReader(new MemoryStream(buffer));
                var opCode = (RpcPacketType)reader.ReadInt32();
                var dataLength = reader.ReadInt32();
                var data = Encoding.UTF8.GetString(reader.ReadBytes(dataLength));

                OnMessageReceivedIPC(opCode, data);
            }
        }

        private void OnMessageReceivedIPC(RpcPacketType opCode, string data)
        {
            if (opCode == RpcPacketType.PING)
            {
                SendPacketIPC(data, RpcPacketType.PONG);
                return;
            }

            IDiscordResponse basePacket;
            try
            {
                basePacket = JsonConvert.DeserializeObject<IDiscordResponse>(data, _jsonSerializerSettings);
            }
            catch (Exception exc)
            {
                _logger.Error(exc, $"Error deserializing discord message");
                return;
            }

            if (basePacket is CommandDiscordResponse cmd)
            {
                switch (cmd)
                {
                    //we should only receive the authorize event once from the client
                    //since after that the token should be refreshed
                    case AuthorizeDiscordResponse authorize:
                        TokenResponse response;
                        if (token.Value == null)
                        {
                            //if we do not have a cached token, get a new one
                            response = GetAccessTokenAsync(authorize.Data.Code).Result;
                        }
                        else
                        {
                            //if we have a cached token, check if it expired
                            if (token.Value.ExpirationDate > DateTime.UtcNow)
                            {
                                return;
                            }
                            //if it expired, refresh it
                            else
                            {
                                response = RefreshAccessTokenAsync(token.Value.RefreshToken).Result;
                            }
                        }
                        token.Value = new SavedToken
                        {
                            AccessToken = response.AccessToken,
                            RefreshToken = response.RefreshToken,
                            ExpirationDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn)
                        };
                        token.Save();
                        SendPacketIPC(new DiscordRequest(DiscordRpcCommand.AUTHENTICATE).WithArgument("access_token", token.Value.AccessToken));
                        break;
                    case AuthenticateDiscordResponse authenticate:
                        DataModel.User.Username = authenticate.Data.User.Username;
                        DataModel.User.Discriminator = authenticate.Data.User.Discriminator;
                        DataModel.User.Id = authenticate.Data.User.Id;

                        //Initial request for data, then use events after
                        SendPacketIPC(new DiscordRequest(DiscordRpcCommand.GET_VOICE_SETTINGS));
                        SendPacketIPC(new DiscordRequest(DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL));

                        //Subscribe to these events as well
                        SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.VOICE_SETTINGS_UPDATE));
                        SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.NOTIFICATION_CREATE));
                        SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.VOICE_CONNECTION_STATUS));
                        SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.VOICE_CHANNEL_SELECT));
                        break;
                    case VoiceSettingsDiscordResponse voice:
                        DataModel.VoiceSettings.Deafened = voice.Data.Deaf;
                        DataModel.VoiceSettings.Muted = voice.Data.Mute;
                        break;
                    case SubscribeDiscordResponse subscribe:
                        _logger.Verbose($"Subscribed to event {subscribe.Data.Event} successfully.");
                        break;

                    case SelectedVoiceChannelDiscordResponse selectedVoiceChannel:
                        //Data is null when the user leaves a voice channel
                        if (selectedVoiceChannel.Data != null)
                        {
                            SubscribeToSpeakingEvents(selectedVoiceChannel.Data.Id);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (basePacket is EventDiscordResponse evt)
            {

                switch (evt)
                {
                    case ReadyDiscordResponse:
                        if (token.Value != null && token.Value.ExpirationDate > DateTime.Now)
                        {

                            //TODO: maybe we dont even need to check for expiration in this if.
                            //maybe can refresh + authenticate instead of authorizing again.
                            //i think authorize should only be done if token.Value is actually null.
                            //investigate
                            SendPacketIPC(new DiscordRequest(DiscordRpcCommand.AUTHENTICATE)
                                                .WithArgument("access_token", token.Value.AccessToken));
                        }
                        else
                        {
                            SendPacketIPC(new DiscordRequest(DiscordRpcCommand.AUTHORIZE)
                                                .WithArgument("client_id", clientId.Value)
                                                .WithArgument("scopes", _scopes));
                        }
                        break;
                    case VoiceSettingsUpdateDiscordResponse voice:
                        DataModel.VoiceSettings.Deafened = voice.Data.Deaf;
                        DataModel.VoiceSettings.Muted = voice.Data.Mute;
                        break;
                    case VoiceConnectionStatusDiscordResponse voiceStatus:
                        DataModel.VoiceConnection.State = voiceStatus.Data.State;
                        DataModel.VoiceConnection.Ping = voiceStatus.Data.LastPing;
                        break;
                    case NotificationCreateDiscordResponse:
                        DataModel.Notification.Trigger();
                        break;
                    case SpeakingStopDiscordResponse speakingStop:
                        if (speakingStop.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceSettings.Speaking = false;
                        }
                        break;
                    case SpeakingStartDiscordResponse speakingStart:
                        if (speakingStart.Data.UserId == DataModel.User.Id)
                        {
                            DataModel.VoiceSettings.Speaking = true;
                        }
                        break;
                    case VoiceChannelSelectDiscordResponse voiceSelect:
                        if (voiceSelect.Data.ChannelId is not null)//join voice channel
                        {
                            SubscribeToSpeakingEvents(voiceSelect.Data.ChannelId);
                        }
                        else//leave voice channel
                        {
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SubscribeToSpeakingEvents(string  id)
        {
            SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.SPEAKING_START)
                                .WithArgument("channel_id", id));
            SendPacketIPC(new DiscordSubscribe(DiscordRpcEvent.SPEAKING_STOP)
                                .WithArgument("channel_id", id));
        }
        #endregion

        #region Authorization & Authentication
        private async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
        {
            return await GetCredentials("authorization_code", "code", challengeCode);
        }

        private async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            return await GetCredentials("refresh_token", "refresh_token", refreshToken);
        }

        private async Task<TokenResponse> GetCredentials(string grantType, string secretType, string secret)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
            {
                ["grant_type"] = grantType,
                [secretType] = secret,
                ["client_id"] = clientId.Value,
                ["client_secret"] = clientSecret.Value
            };

            using HttpResponseMessage response = await client.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
            string responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(responseString, _jsonSerializerSettings);
        }
        #endregion
    }
}