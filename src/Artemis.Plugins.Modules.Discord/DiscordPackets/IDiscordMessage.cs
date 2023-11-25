using Artemis.Plugins.Modules.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

[JsonConverter(typeof(JsonSubtypes), "cmd")]
[KnownSubType(typeof(DiscordEvent), DiscordRpcCommand.DISPATCH)]
[KnownSubType(typeof(DiscordAuthorizeResponse), DiscordRpcCommand.AUTHORIZE)]
[KnownSubType(typeof(DiscordAuthenticateResponse), DiscordRpcCommand.AUTHENTICATE)]
[KnownSubType(typeof(DiscordGetVoiceSettingsResponse), DiscordRpcCommand.GET_VOICE_SETTINGS)]
[KnownSubType(typeof(DiscordSubscribeResponse), DiscordRpcCommand.SUBSCRIBE)]
[KnownSubType(typeof(DiscordGetSelectedVoiceChannelResponse), DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL)]
public interface IDiscordMessage
{
}
