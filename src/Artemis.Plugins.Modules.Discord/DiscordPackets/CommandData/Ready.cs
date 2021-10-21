using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord
{
    public record Ready
    (
        [JsonProperty("v")] int Version,
        ReadyConfig Config,
        User User
    );
}
