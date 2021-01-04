using Artemis.Plugins.DataModelExpansions.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(DiscordEvent), DiscordRpcCommand.DISPATCH)]
    [FallBackSubType(typeof(DiscordResponse))]
    public interface IDiscordMessage { }

    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(AuthorizeDiscordResponse), DiscordRpcCommand.AUTHORIZE)]
    [KnownSubType(typeof(AuthenticateDiscordResponse), DiscordRpcCommand.AUTHENTICATE)]
    [KnownSubType(typeof(VoiceSettingsDiscordResponse), DiscordRpcCommand.GET_VOICE_SETTINGS)]
    [KnownSubType(typeof(SubscribeDiscordResponse), DiscordRpcCommand.SUBSCRIBE)]
    [KnownSubType(typeof(SelectedVoiceChannelDiscordResponse), DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL)]
    public class DiscordResponse : IDiscordMessage
    {
        [JsonProperty("cmd")]
        public DiscordRpcCommand Command { get; set; }

        public Guid Nonce { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "evt")]
    [KnownSubType(typeof(ReadyDiscordEvent), DiscordRpcEvent.READY)]
    [KnownSubType(typeof(VoiceSettingsUpdateDiscordEvent), DiscordRpcEvent.VOICE_SETTINGS_UPDATE)]
    [KnownSubType(typeof(VoiceConnectionStatusDiscordEvent), DiscordRpcEvent.VOICE_CONNECTION_STATUS)]
    [KnownSubType(typeof(NotificationCreateDiscordEvent), DiscordRpcEvent.NOTIFICATION_CREATE)]
    [KnownSubType(typeof(SpeakingStopDiscordEvent), DiscordRpcEvent.SPEAKING_STOP)]
    [KnownSubType(typeof(SpeakingStartDiscordEvent), DiscordRpcEvent.SPEAKING_START)]
    [KnownSubType(typeof(VoiceChannelSelectDiscordEvent), DiscordRpcEvent.VOICE_CHANNEL_SELECT)]
    public class DiscordEvent : IDiscordMessage
    {
        [JsonProperty("evt")]
        public DiscordRpcEvent Event { get; set; }
    }
}
