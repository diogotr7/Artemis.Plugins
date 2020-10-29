using JsonSubTypes;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class Passive
    {
        public string DisplayName { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public class Ability
    {
        public int AbilityLevel { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public class Abilities
    {
        public Passive Passive { get; set; }
        public Ability Q { get; set; }
        public Ability W { get; set; }
        public Ability E { get; set; }
        public Ability R { get; set; }
    }

    public class ChampionStats
    {
        public float AbilityPower { get; set; }
        public float Armor { get; set; }
        public float ArmorPenetrationFlat { get; set; }
        public float ArmorPenetrationPercent { get; set; }
        public float AttackDamage { get; set; }
        public float AttackRange { get; set; }
        public float AttackSpeed { get; set; }
        public float BonusArmorPenetrationPercent { get; set; }
        public float BonusMagicPenetrationPercent { get; set; }
        public float CooldownReduction { get; set; }
        public float CritChance { get; set; }
        public float CritDamage { get; set; }
        public float CurrentHealth { get; set; }
        public float HealthRegenRate { get; set; }
        public float LifeSteal { get; set; }
        public float MagicLethality { get; set; }
        public float MagicPenetrationFlat { get; set; }
        public float MagicPenetrationPercent { get; set; }
        public float MagicResist { get; set; }
        public float MaxHealth { get; set; }
        public float MoveSpeed { get; set; }
        public float PhysicalLethality { get; set; }
        public float ResourceMax { get; set; }
        public float ResourceRegenRate { get; set; }
        public string ResourceType { get; set; }
        public float ResourceValue { get; set; }
        public float SpellVamp { get; set; }
        public float Tenacity { get; set; }
    }

    public class Rune
    {
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public class StatRune
    {
        public int Id { get; set; }
        public string RawDescription { get; set; }
    }

    public class FullRunes
    {
        public Rune[] GeneralRunes { get; set; }
        public Rune Keystone { get; set; }
        public Rune PrimaryRuneTree { get; set; }
        public Rune SecondaryRuneTree { get; set; }
        public StatRune[] StatRunes { get; set; }
    }

    public class ActivePlayer
    {
        public Abilities Abilities { get; set; }
        public ChampionStats ChampionStats { get; set; }
        public float CurrentGold { get; set; }
        public FullRunes FullRunes { get; set; }
        public int Level { get; set; }
        public string SummonerName { get; set; }
    }

    public class Item
    {
        public bool CanUse { get; set; }
        public bool Consumable { get; set; }
        public int Count { get; set; }
        public string DisplayName { get; set; }
        public int ItemID { get; set; }
        public int Price { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
        public int Slot { get; set; }
    }

    public class Runes
    {
        public Rune Keystone { get; set; }
        public Rune PrimaryRuneTree { get; set; }
        public Rune SecondaryRuneTree { get; set; }
    }

    public class Scores
    {
        public int Assists { get; set; }
        public int CreepScore { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public float WardScore { get; set; }
    }

    public class SummonerSpell
    {
        public string DisplayName { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public class SummonerSpells
    {
        public SummonerSpell SummonerSpellOne { get; set; }
        public SummonerSpell SummonerSpellTwo { get; set; }
    }

    public class AllPlayer
    {
        public string ChampionName { get; set; }
        public bool IsBot { get; set; }
        public bool IsDead { get; set; }
        public Item[] Items { get; set; }
        public int Level { get; set; }
        public string Position { get; set; }
        public string RawChampionName { get; set; }
        public float RespawnTimer { get; set; }
        public Runes Runes { get; set; }
        public Scores Scores { get; set; }
        public int SkinID { get; set; }
        public string SummonerName { get; set; }
        public SummonerSpells SummonerSpells { get; set; }
        public string Team { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "EventName")]
    [JsonSubtypes.KnownSubType(typeof(BaronKillEvent), "BaronKill")]
    [JsonSubtypes.KnownSubType(typeof(HeraldKillEvent), "HeraldKill")]
    [JsonSubtypes.KnownSubType(typeof(DragonKillEvent), "DragonKill")]
    [JsonSubtypes.KnownSubType(typeof(ChampionKillEvent), "ChampionKill")]
    [JsonSubtypes.KnownSubType(typeof(MultikillEvent), "Multikill")]
    [JsonSubtypes.KnownSubType(typeof(FirstBloodEvent), "FirstBlood")]
    [JsonSubtypes.KnownSubType(typeof(AceEvent), "Ace")]
    [JsonSubtypes.KnownSubType(typeof(InhibKillEvent), "InhibKilled")]
    [JsonSubtypes.KnownSubType(typeof(TurretKillEvent), "TurretKilled")]
    [JsonSubtypes.KnownSubType(typeof(GameStartEvent), "GameStart")]
    [JsonSubtypes.KnownSubType(typeof(GameEndEvent), "GameEnd")]
    [JsonSubtypes.KnownSubType(typeof(MinionsSpawningEvent), "MinionsSpawning")]
    [JsonSubtypes.KnownSubType(typeof(FirstBrickEvent), "FirstBrick")]
    [JsonSubtypes.KnownSubType(typeof(InhibRespawningSoonEvent), "InhibRespawningSoon")]
    [JsonSubtypes.KnownSubType(typeof(InhibRespawnedEvent), "InhibRespawned")]
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public float EventTime { get; set; }
    }

    public class GameStartEvent : Event { }

    public class GameEndEvent : Event
    {
        public string Result { get; set; }
    }

    public class InhibRespawningSoonEvent : Event
    {
        public string InhibRespawningSoon { get; set; }
    }

    public class InhibRespawnedEvent : Event {
        public string InhibRespawned { get; set; }
    }

    public class MinionsSpawningEvent : Event { }

    public class FirstBrickEvent : Event { }

    public class BaronKillEvent : Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class HeraldKillEvent : Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class DragonKillEvent : Event
    {
        public string DragonType { get; set; }
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
    public class FirstBloodEvent : Event
    {
        public string Recipient { get; set; }
    }

    public class MultikillEvent : Event
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }

    public class AceEvent : Event
    {
        public string Acer { get; set; }
        public string AcingTeam { get; set; }
    }

    public class InhibKillEvent : Event
    {
        public string KillerName { get; set; }
        public string InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class TurretKillEvent : Event
    {
        public string KillerName { get; set; }
        public string TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class EventList
    {
        public Event[] Events { get; set; }
    }

    public class GameData
    {
        public string GameMode { get; set; }
        public float GameTime { get; set; }
        public string MapName { get; set; }
        public int MapNumber { get; set; }
        public string MapTerrain { get; set; }
    }

    public class RootGameData
    {
        public ActivePlayer ActivePlayer { get; set; }
        public AllPlayer[] AllPlayers { get; set; }
        public EventList Events { get; set; }
        public GameData GameData { get; set; }
    }
}
