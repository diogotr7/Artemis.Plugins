using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class LeagueOfLegendsDataModel : DataModel
    {
        public PlayerDataModel Player { get; set; }
        public MatchDataModel Match { get; set; }
        internal RootGameData RootGameData { get; set; }

        public LeagueOfLegendsDataModel()
        {
            Player = new PlayerDataModel(this);
            Match = new MatchDataModel(this);
        }
    }
}
