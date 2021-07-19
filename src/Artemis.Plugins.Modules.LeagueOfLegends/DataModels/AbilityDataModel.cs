using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityDataModel
    {
        private readonly Func<Ability> ability;

        public AbilityDataModel(Func<Ability> accessor) => ability = accessor;

        public string Name => ability().DisplayName;
        public int Level => ability().AbilityLevel;
        public bool Learned => Level > 0;
    }
}
