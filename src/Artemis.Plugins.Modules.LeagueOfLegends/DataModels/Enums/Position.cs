﻿
using Artemis.Plugins.Modules.LeagueOfLegends.Utils;

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
        [Name("UTILITY")]
        Support
    }
}
