using Artemis.Core;
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
        public DataModelEvent Ace { get; set; } = new DataModelEvent();
        public DataModelEvent BaronKill { get; set; } = new DataModelEvent();
        public DataModelEvent ChampionKill { get; set; } = new DataModelEvent();
        public DataModelEvent DragonKill { get; set; } = new DataModelEvent();
        public DataModelEvent FirstBlood { get; set; } = new DataModelEvent();
        public DataModelEvent FirstBrick { get; set; } = new DataModelEvent();
        public DataModelEvent GameEnd { get; set; } = new DataModelEvent();
        public DataModelEvent GameStart { get; set; } = new DataModelEvent();
        public DataModelEvent HeraldKill { get; set; } = new DataModelEvent();
        public DataModelEvent InhibKill { get; set; } = new DataModelEvent();
        public DataModelEvent InhibRespawned { get; set; } = new DataModelEvent();
        public DataModelEvent InhibRespawningSoon { get; set; } = new DataModelEvent();
        public DataModelEvent MinionsSpawning { get; set; } = new DataModelEvent();
        public DataModelEvent Multikill { get; set; } = new DataModelEvent();
        public DataModelEvent TurretKill { get; set; } = new DataModelEvent();
    }
}
