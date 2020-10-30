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
        public DataModelEvent<AceEventArgs> Ace { get; set; } = new DataModelEvent<AceEventArgs>();
        public DataModelEvent<EpicCreatureKillEventArgs> BaronKill { get; set; } = new DataModelEvent<EpicCreatureKillEventArgs>();
        public DataModelEvent<ChampionKillEventArgs> ChampionKill { get; set; } = new DataModelEvent<ChampionKillEventArgs>();
        public DataModelEvent<DragonKillEventArgs> DragonKill { get; set; } = new DataModelEvent<DragonKillEventArgs>();
        public DataModelEvent<FirstBloodEventArgs> FirstBlood { get; set; } = new DataModelEvent<FirstBloodEventArgs>();
        public DataModelEvent FirstBrick { get; set; } = new DataModelEvent();
        public DataModelEvent<GameEndEventArgs> GameEnd { get; set; } = new DataModelEvent<GameEndEventArgs>();
        public DataModelEvent GameStart { get; set; } = new DataModelEvent();
        public DataModelEvent<EpicCreatureKillEventArgs> HeraldKill { get; set; } = new DataModelEvent<EpicCreatureKillEventArgs>();
        public DataModelEvent<InhibKillEventArgs> InhibKill { get; set; } = new DataModelEvent<InhibKillEventArgs>();
        public DataModelEvent<InhibRespawnedEventArgs> InhibRespawned { get; set; } = new DataModelEvent<InhibRespawnedEventArgs>();
        public DataModelEvent<InhibRespawningSoonEventArgs> InhibRespawningSoon { get; set; } = new DataModelEvent<InhibRespawningSoonEventArgs>();
        public DataModelEvent MinionsSpawning { get; set; } = new DataModelEvent();
        public DataModelEvent<MultikillEventArgs> Multikill { get; set; } = new DataModelEvent<MultikillEventArgs>();
        public DataModelEvent<TurretKillEventArgs> TurretKill { get; set; } = new DataModelEvent<TurretKillEventArgs>();
    }
}
