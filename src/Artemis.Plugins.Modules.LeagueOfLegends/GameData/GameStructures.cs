namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public struct Passive
    {
        public string DisplayName { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public struct Ability
    {
        public int AbilityLevel { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public struct Abilities
    {
        public Passive Passive { get; set; }
        public Ability Q { get; set; }
        public Ability W { get; set; }
        public Ability E { get; set; }
        public Ability R { get; set; }
    }

    public struct ChampionStats
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

    public struct Rune
    {
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public struct StatRune
    {
        public int Id { get; set; }
        public string RawDescription { get; set; }
    }

    public struct FullRunes
    {
        public Rune[] GeneralRunes { get; set; }
        public Rune Keystone { get; set; }
        public Rune PrimaryRuneTree { get; set; }
        public Rune SecondaryRuneTree { get; set; }
        public StatRune[] StatRunes { get; set; }
    }

    public struct ActivePlayer
    {
        public Abilities Abilities { get; set; }
        public ChampionStats ChampionStats { get; set; }
        public float CurrentGold { get; set; }
        public FullRunes FullRunes { get; set; }
        public int Level { get; set; }
        public string SummonerName { get; set; }
    }

    public struct Item
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

    public struct Runes
    {
        public Rune Keystone { get; set; }
        public Rune PrimaryRuneTree { get; set; }
        public Rune SecondaryRuneTree { get; set; }
    }

    public struct Scores
    {
        public int Assists { get; set; }
        public int CreepScore { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public float WardScore { get; set; }
    }

    public struct SummonerSpell
    {
        public string DisplayName { get; set; }
        public string RawDescription { get; set; }
        public string RawDisplayName { get; set; }
    }

    public struct SummonerSpells
    {
        public SummonerSpell SummonerSpellOne { get; set; }
        public SummonerSpell SummonerSpellTwo { get; set; }
    }

    public struct AllPlayer
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

    public struct EventList
    {
        public LolEvent[] Events { get; set; }
    }

    public struct GameData
    {
        public string GameMode { get; set; }
        public float GameTime { get; set; }
        public string MapName { get; set; }
        public int MapNumber { get; set; }
        public string MapTerrain { get; set; }
    }

    public struct RootGameData
    {
        public ActivePlayer ActivePlayer { get; set; }
        public AllPlayer[] AllPlayers { get; set; }
        public EventList Events { get; set; }
        public GameData GameData { get; set; }
    }
}
