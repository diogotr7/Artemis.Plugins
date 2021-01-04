using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class ReadyDiscordEvent : DiscordEvent
    {
        public ReadyObject Data { get; set; }
    }

    public class ReadyConfig
    {
        public string CdnHost { get; set; }

        public string ApiEndpoint { get; set; }

        public string Enviroment { get; set; }
    }

    public class ReadyObject
    {
        [JsonProperty("v")]
        public int Version { get; set; }

        public ReadyConfig Config { get; set; }

        public DiscordUser User { get; set; }
    }
}
