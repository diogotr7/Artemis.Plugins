using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class AbilityDataModel : DataModel
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public bool Learned => Level > 0;

        internal void Reset()
        {
            Name = "";
            Level = -1;
        }
    }
}
