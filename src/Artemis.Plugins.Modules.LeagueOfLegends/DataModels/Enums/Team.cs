using Artemis.Plugins.Modules.LeagueOfLegends.Utils;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums
{
    public enum Team
    {
        Unknown = -1,
        None = 0,
        [Name("ORDER")]
        Blue,
        [Name("CHAOS")]
        Red
    }
}
