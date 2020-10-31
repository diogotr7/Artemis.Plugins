using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums
{
    public enum DragonType
    {
        Unknown = -1,
        [Name("Air")]
        Cloud,
        [Name("Fire")]
        Infernal,
        [Name("Water")]
        Ocean,
        [Name("Earth")]
        Mountain,
        Elder
    }
}
