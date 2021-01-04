using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class SubscribeDiscordResponse : DiscordResponse
    {
        public SubscribeData Data { get; set; }
    }

    public class SubscribeData
    {
        [JsonProperty("evt")]
        public string Event { get; set; }
    }
}