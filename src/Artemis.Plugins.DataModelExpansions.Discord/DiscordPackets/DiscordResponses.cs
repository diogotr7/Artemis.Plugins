using Artemis.Plugins.DataModelExpansions.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(DiscordEvent<>), DiscordRpcCommand.DISPATCH)]
    [FallBackSubType(typeof(DiscordResponse<>))]
    public interface IDiscordMessage { }

    public class DiscordResponse : IDiscordMessage
    {
        [JsonProperty("cmd")]
        public DiscordRpcCommand Command { get; set; }

        public Guid Nonce { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(DiscordResponse<AuthorizeData>), DiscordRpcCommand.AUTHORIZE)]
    [KnownSubType(typeof(DiscordResponse<AuthenticateData>), DiscordRpcCommand.AUTHENTICATE)]
    [KnownSubType(typeof(DiscordResponse<VoiceSettingsData>), DiscordRpcCommand.GET_VOICE_SETTINGS)]
    [KnownSubType(typeof(DiscordResponse<SubscribeData>), DiscordRpcCommand.SUBSCRIBE)]
    [KnownSubType(typeof(DiscordResponse<SelectedVoiceChannelData>), DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL)]
    public class DiscordResponse<T> : DiscordResponse
    {
        public T Data { get; init; }
    }

    public class DiscordEvent : IDiscordMessage
    {
        [JsonProperty("evt")]
        public DiscordRpcEvent Event { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "evt")]
    [KnownSubType(typeof(DiscordEvent<ReadyData>), DiscordRpcEvent.READY)]
    [KnownSubType(typeof(DiscordEvent<VoiceSettingsData>), DiscordRpcEvent.VOICE_SETTINGS_UPDATE)]
    [KnownSubType(typeof(DiscordEvent<VoiceConnectionStatusData>), DiscordRpcEvent.VOICE_CONNECTION_STATUS)]
    [KnownSubType(typeof(DiscordEvent<NotificationCreateData>), DiscordRpcEvent.NOTIFICATION_CREATE)]
    [KnownSubType(typeof(DiscordEvent<SpeakingStopData>), DiscordRpcEvent.SPEAKING_STOP)]
    [KnownSubType(typeof(DiscordEvent<SpeakingStartData>), DiscordRpcEvent.SPEAKING_START)]
    [KnownSubType(typeof(DiscordEvent<VoiceChannelSelectData>), DiscordRpcEvent.VOICE_CHANNEL_SELECT)]
    public class DiscordEvent<T> : DiscordEvent
    {
        public T Data { get; init; }
    }
}
