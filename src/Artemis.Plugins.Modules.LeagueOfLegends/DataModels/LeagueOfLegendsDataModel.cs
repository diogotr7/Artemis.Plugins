using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class LeagueOfLegendsDataModel : DataModel
    {
        public PlayerDataModel Player { get; } = new();
        public MatchDataModel Match { get; } = new();

        public void Apply(RootGameData rootGameData)
        {
            Player.Apply(rootGameData);
            Match.Apply(rootGameData);
        }
    }
}
