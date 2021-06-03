using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord
{
    public record ReadyConfig
    (
        string CdnHost,
        string ApiEndpoint,
        string Enviroment
    );

    public record ReadyData(
        [JsonProperty("v")] int Version,
        ReadyConfig Config,
        DiscordUser User
    );
}
