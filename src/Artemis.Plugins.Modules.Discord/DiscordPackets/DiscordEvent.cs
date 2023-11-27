using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;
using Artemis.Plugins.Modules.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

[JsonConverter(typeof(JsonSubtypes), "evt")]
[KnownSubType(typeof(DiscordReadyEvent), DiscordRpcEvent.READY)]
[KnownSubType(typeof(DiscordVoiceSettingsUpdateEvent), DiscordRpcEvent.VOICE_SETTINGS_UPDATE)]
[KnownSubType(typeof(DiscordVoiceConnectionStatusEvent), DiscordRpcEvent.VOICE_CONNECTION_STATUS)]
[KnownSubType(typeof(DiscordNotificationCreateEvent), DiscordRpcEvent.NOTIFICATION_CREATE)]
[KnownSubType(typeof(DiscordSpeakingStopEvent), DiscordRpcEvent.SPEAKING_STOP)]
[KnownSubType(typeof(DiscordSpeakingStartEvent), DiscordRpcEvent.SPEAKING_START)]
[KnownSubType(typeof(DiscordVoiceChannelSelectEvent), DiscordRpcEvent.VOICE_CHANNEL_SELECT)]
[KnownSubType(typeof(DiscordVoiceStateCreateEvent), DiscordRpcEvent.VOICE_STATE_CREATE)]
[KnownSubType(typeof(DiscordVoiceStateUpdateEvent), DiscordRpcEvent.VOICE_STATE_UPDATE)]
[KnownSubType(typeof(DiscordVoiceStateDeleteEvent), DiscordRpcEvent.VOICE_STATE_DELETE)]
public class DiscordEvent : IDiscordMessage
{
    [JsonProperty("evt")] 
    public DiscordRpcEvent Event { get; set; }
}

public abstract class DiscordEvent<T> : DiscordEvent
{
#pragma warning disable CS8618
    public T Data { get; set; }
#pragma warning restore CS8618
}

public sealed class DiscordReadyEvent : DiscordEvent<Ready> { }
public sealed class DiscordVoiceSettingsUpdateEvent : DiscordEvent<VoiceSettings> { }
public sealed class DiscordVoiceConnectionStatusEvent : DiscordEvent<VoiceConnectionStatus> { }
public sealed class DiscordNotificationCreateEvent : DiscordEvent<Notification> { }
public sealed class DiscordSpeakingStartEvent : DiscordEvent<SpeakingStartStop> { }
public sealed class DiscordSpeakingStopEvent : DiscordEvent<SpeakingStartStop> { }
public sealed class DiscordVoiceChannelSelectEvent : DiscordEvent<VoiceChannelSelect> { }
public sealed class DiscordVoiceStateCreateEvent : DiscordEvent<UserVoiceState> { }
public sealed class DiscordVoiceStateUpdateEvent : DiscordEvent<UserVoiceState> { }
public sealed class DiscordVoiceStateDeleteEvent : DiscordEvent<UserVoiceState> { }