using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityGroupDataModel : DataModel
    {
        public AbilityDataModel Q { get; set; } = new AbilityDataModel();
        public AbilityDataModel W { get; set; } = new AbilityDataModel();
        public AbilityDataModel E { get; set; } = new AbilityDataModel();
        public AbilityDataModel R { get; set; } = new AbilityDataModel();

        internal void Reset()
        {
            Q.Reset();
            W.Reset();
            E.Reset();
            R.Reset();
        }
    }
}
