using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public record SubscribeData(
        [JsonProperty("evt")]
        string Event
    ) : DiscordMessageData;
}