using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    [JsonConverter(typeof(JsonSubtypes), "EventName")]
    [KnownSubType(typeof(AceEvent), "Ace")]
    [KnownSubType(typeof(BaronKillEvent), "BaronKill")]
    [KnownSubType(typeof(ChampionKillEvent), "ChampionKill")]
    [KnownSubType(typeof(DragonKillEvent), "DragonKill")]
    [KnownSubType(typeof(FirstBloodEvent), "FirstBlood")]
    [KnownSubType(typeof(FirstBrickEvent), "FirstBrick")]
    [KnownSubType(typeof(GameEndEvent), "GameEnd")]
    [KnownSubType(typeof(GameStartEvent), "GameStart")]
    [KnownSubType(typeof(HeraldKillEvent), "HeraldKill")]
    [KnownSubType(typeof(InhibKillEvent), "InhibKilled")]
    [KnownSubType(typeof(InhibRespawnedEvent), "InhibRespawned")]
    [KnownSubType(typeof(InhibRespawningSoonEvent), "InhibRespawningSoon")]
    [KnownSubType(typeof(MinionsSpawningEvent), "MinionsSpawning")]
    [KnownSubType(typeof(MultikillEvent), "Multikill")]
    [KnownSubType(typeof(TurretKillEvent), "TurretKilled")]
    public class LolEvent
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public float EventTime { get; set; }
    }
}
