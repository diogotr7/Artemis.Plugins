using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class LeagueOfLegendsDataModel : DataModel
    {
        public PlayerDataModel Player { get; set; } = new PlayerDataModel();
        public MatchDataModel Match { get; set; } = new MatchDataModel();

        internal void Reset()
        {
            Player.Reset();
            Match.Reset();
        }
    }
}
