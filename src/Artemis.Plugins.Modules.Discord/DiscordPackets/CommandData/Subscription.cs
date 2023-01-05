using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record Subscription(
    [JsonProperty("evt")]
    string Event
);