using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;
using Artemis.Plugins.Modules.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

public class DiscordEvent : IDiscordMessage
{
    [JsonProperty("evt")]
    public DiscordRpcEvent Event { get; set; }
}

[JsonConverter(typeof(JsonSubtypes), "evt")]
[KnownSubType(typeof(DiscordEvent<Ready>), DiscordRpcEvent.READY)]
[KnownSubType(typeof(DiscordEvent<VoiceSettings>), DiscordRpcEvent.VOICE_SETTINGS_UPDATE)]
[KnownSubType(typeof(DiscordEvent<VoiceConnectionStatus>), DiscordRpcEvent.VOICE_CONNECTION_STATUS)]
[KnownSubType(typeof(DiscordEvent<Notification>), DiscordRpcEvent.NOTIFICATION_CREATE)]
[KnownSubType(typeof(DiscordEvent<SpeakingStartStop>), DiscordRpcEvent.SPEAKING_STOP)]
[KnownSubType(typeof(DiscordEvent<SpeakingStartStop>), DiscordRpcEvent.SPEAKING_START)]
[KnownSubType(typeof(DiscordEvent<VoiceChannelSelect>), DiscordRpcEvent.VOICE_CHANNEL_SELECT)]
[KnownSubType(typeof(DiscordEvent<UserVoiceState>), DiscordRpcEvent.VOICE_STATE_CREATE)]
[KnownSubType(typeof(DiscordEvent<UserVoiceState>), DiscordRpcEvent.VOICE_STATE_UPDATE)]
[KnownSubType(typeof(DiscordEvent<UserVoiceState>), DiscordRpcEvent.VOICE_STATE_DELETE)]
public class DiscordEvent<T> : DiscordEvent
{
    public T Data { get; init; }
}
