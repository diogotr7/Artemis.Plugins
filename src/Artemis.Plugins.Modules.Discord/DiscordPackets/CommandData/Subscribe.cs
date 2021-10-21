using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord
{
    public record Subscribe(
        [JsonProperty("evt")]
        string Event
    );
}