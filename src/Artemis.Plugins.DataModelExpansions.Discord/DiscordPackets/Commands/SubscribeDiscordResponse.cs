using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class SubscribeDiscordResponse : DiscordResponse
    {
        public SubscribeData Data { get; set; }
    }

    public record SubscribeData(
        [JsonProperty("evt")]
        string Event
    );
}