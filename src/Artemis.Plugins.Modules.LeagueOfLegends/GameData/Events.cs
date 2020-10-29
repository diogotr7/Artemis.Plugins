using JsonSubTypes;
using Newtonsoft.Json;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
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
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public float EventTime { get; set; }
    }

    public class AceEvent : Event
    {
        public string Acer { get; set; }
        public string AcingTeam { get; set; }
    }

    public class BaronKillEvent : Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class ChampionKillEvent : Event
    {
        public string KillerName { get; set; }
        public string VictimName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class DragonKillEvent : Event
    {
        public string DragonType { get; set; }
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class FirstBloodEvent : Event
    {
        public string Recipient { get; set; }
    }

    public class FirstBrickEvent : Event { }

    public class GameEndEvent : Event
    {
        public string Result { get; set; }
    }

    public class GameStartEvent : Event { }

    public class HeraldKillEvent : Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class InhibKillEvent : Event
    {
        public string KillerName { get; set; }
        public string InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class InhibRespawnedEvent : Event
    {
        public string InhibRespawned { get; set; }
    }

    public class InhibRespawningSoonEvent : Event
    {
        public string InhibRespawningSoon { get; set; }
    }

    public class MinionsSpawningEvent : Event { }

    public class MultikillEvent : Event
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }

    public class TurretKillEvent : Event
    {
        public string KillerName { get; set; }
        public string TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }
}
