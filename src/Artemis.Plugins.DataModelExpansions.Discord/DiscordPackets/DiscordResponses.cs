using Artemis.Plugins.DataModelExpansions.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(EventDiscordResponse), DiscordRpcCommand.DISPATCH)]
    [FallBackSubType(typeof(CommandDiscordResponse))]
    public interface IDiscordResponse
    {

    }

    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(AuthorizeDiscordResponse), DiscordRpcCommand.AUTHORIZE)]
    [KnownSubType(typeof(AuthenticateDiscordResponse), DiscordRpcCommand.AUTHENTICATE)]
    [KnownSubType(typeof(VoiceSettingsDiscordResponse), DiscordRpcCommand.GET_VOICE_SETTINGS)]
    [KnownSubType(typeof(SubscribeDiscordResponse), DiscordRpcCommand.SUBSCRIBE)]
    [KnownSubType(typeof(SelectedVoiceChannelDiscordResponse), DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL)]
    public class CommandDiscordResponse : IDiscordResponse
    {
        [JsonProperty("cmd")]
        public DiscordRpcCommand Command { get; set; }

        public Guid Nonce { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "evt")]
    [KnownSubType(typeof(ReadyDiscordResponse), DiscordRpcEvent.READY)]
    [KnownSubType(typeof(VoiceSettingsUpdateDiscordResponse), DiscordRpcEvent.VOICE_SETTINGS_UPDATE)]
    [KnownSubType(typeof(VoiceConnectionStatusDiscordResponse), DiscordRpcEvent.VOICE_CONNECTION_STATUS)]
    [KnownSubType(typeof(NotificationCreateDiscordResponse), DiscordRpcEvent.NOTIFICATION_CREATE)]
    [KnownSubType(typeof(SpeakingStopDiscordResponse), DiscordRpcEvent.SPEAKING_STOP)]
    [KnownSubType(typeof(SpeakingStartDiscordResponse), DiscordRpcEvent.SPEAKING_START)]
    [KnownSubType(typeof(VoiceChannelSelectDiscordResponse), DiscordRpcEvent.VOICE_CHANNEL_SELECT)]
    public class EventDiscordResponse : IDiscordResponse
    {
        [JsonProperty("evt")]
        public DiscordRpcEvent Event { get; set; }
    }
}
