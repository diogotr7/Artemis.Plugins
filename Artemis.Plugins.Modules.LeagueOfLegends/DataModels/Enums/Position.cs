using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums
{
    public enum Position
    {
        Unknown = -1,
        None = 0,
        Top,
        Jungle,
        Middle,
        Bottom,
        [JsonProperty("UTILITY")]
        Support
    }
}
