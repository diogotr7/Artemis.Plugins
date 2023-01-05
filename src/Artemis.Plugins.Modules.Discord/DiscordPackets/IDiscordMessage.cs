using Artemis.Plugins.Modules.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

[JsonConverter(typeof(JsonSubtypes), "cmd")]
[KnownSubType(typeof(DiscordEvent<>), DiscordRpcCommand.DISPATCH)]
[FallBackSubType(typeof(DiscordResponse<>))]
public interface IDiscordMessage { }
