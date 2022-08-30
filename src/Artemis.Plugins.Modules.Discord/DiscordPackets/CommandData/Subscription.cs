using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord
{
    public record Subscription(
        [JsonProperty("evt")]
        string Event
    );
}