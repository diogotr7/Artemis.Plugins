using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class MatchDataModel : ChildDataModel
    {
        public MatchDataModel(LeagueOfLegendsDataModel root) : base(root) { }

        public MapTerrain MapTerrain => ParseEnum<MapTerrain>.TryParseOr(RootGameData.GameData.MapTerrain, MapTerrain.Unknown);
        public GameMode GameMode => ParseEnum<GameMode>.TryParseOr(RootGameData.GameData.GameMode, GameMode.Unknown);
        public float GameTime => RootGameData.GameData.GameTime;
    }
}
