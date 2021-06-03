using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord
{
    public record SubscribeData(
        [JsonProperty("evt")]
        string Event
    );
}