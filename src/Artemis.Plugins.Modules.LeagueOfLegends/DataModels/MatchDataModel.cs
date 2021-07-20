using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.Utils;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class MatchDataModel : DataModel
    {
        public MapTerrain MapTerrain { get; set; }
        public GameMode GameMode { get; set; }
        public float GameTime { get; set; }
        public DataModelEvent<AceEventArgs> Ace { get; } = new();
        public DataModelEvent<EpicCreatureKillEventArgs> BaronKill { get; } = new();
        public DataModelEvent<ChampionKillEventArgs> ChampionKill { get; } = new();
        public DataModelEvent<DragonKillEventArgs> DragonKill { get; } = new();
        public DataModelEvent<FirstBloodEventArgs> FirstBlood { get; } = new();
        public DataModelEvent FirstBrick { get; } = new();
        public DataModelEvent<GameEndEventArgs> GameEnd { get; } = new();
        public DataModelEvent GameStart { get; } = new();
        public DataModelEvent<EpicCreatureKillEventArgs> HeraldKill { get; } = new();
        public DataModelEvent<InhibKillEventArgs> InhibKill { get; } = new();
        public DataModelEvent<InhibRespawnedEventArgs> InhibRespawned { get; } = new();
        public DataModelEvent<InhibRespawningSoonEventArgs> InhibRespawningSoon { get; } = new();
        public DataModelEvent MinionsSpawning { get; } = new();
        public DataModelEvent<MultikillEventArgs> Multikill { get; } = new();
        public DataModelEvent<TurretKillEventArgs> TurretKill { get; } = new();

        public void Apply(RootGameData rootGameData)
        {
            MapTerrain = ParseEnum<MapTerrain>.TryParseOr(rootGameData.GameData.MapTerrain, MapTerrain.Unknown);
            GameMode = ParseEnum<GameMode>.TryParseOr(rootGameData.GameData.GameMode, GameMode.Unknown);
            GameTime = rootGameData.GameData.GameTime;
        }
    }
}
