using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityGroupDataModel : DataModel
    {
        public AbilityDataModel Q { get; } = new();
        public AbilityDataModel W { get; } = new();
        public AbilityDataModel E { get; } = new();
        public AbilityDataModel R { get; } = new();

        public void Apply(Abilities abilities)
        {
            Q.Apply(abilities.Q);
            W.Apply(abilities.W);
            E.Apply(abilities.E);
            R.Apply(abilities.R);
        }
    }
}
