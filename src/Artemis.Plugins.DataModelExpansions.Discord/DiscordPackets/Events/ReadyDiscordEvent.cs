using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class ReadyDiscordEvent : DiscordEvent
    {
        public ReadyObject Data { get; set; }
    }

    public record ReadyConfig
    (
        string CdnHost ,
        string ApiEndpoint ,
        string Enviroment 
    );

    public record ReadyObject(
        [JsonProperty("v")] int Version,
        ReadyConfig ReadyConfig,
        DiscordUser User
    );
}
