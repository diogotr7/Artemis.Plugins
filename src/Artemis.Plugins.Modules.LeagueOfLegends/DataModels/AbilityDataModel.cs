using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityDataModel : DataModel
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public bool Learned => Level > 0;

        public void Apply(Ability ability)
        {
            Name = ability.DisplayName;
            Level = ability.AbilityLevel;
        }
    }
}
