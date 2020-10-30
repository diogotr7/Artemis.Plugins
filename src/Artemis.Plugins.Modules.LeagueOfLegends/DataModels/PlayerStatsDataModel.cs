using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerStatsDataModel : ChildDataModel
    {
        public PlayerStatsDataModel(LeagueOfLegendsDataModel root) : base(root) { }

        public float AbilityPower => RootGameData.ActivePlayer.ChampionStats.AbilityPower;
        public float Armor => RootGameData.ActivePlayer.ChampionStats.Armor;
        public float ArmorPenetrationFlat => RootGameData.ActivePlayer.ChampionStats.ArmorPenetrationFlat;
        public float ArmorPenetrationPercent => RootGameData.ActivePlayer.ChampionStats.ArmorPenetrationPercent * 100f;
        public float AttackDamage => RootGameData.ActivePlayer.ChampionStats.AttackDamage;
        public float AttackRange => RootGameData.ActivePlayer.ChampionStats.AttackRange;
        public float AttackSpeed => RootGameData.ActivePlayer.ChampionStats.AttackSpeed;
        public float BonusArmorPenetrationPercent => RootGameData.ActivePlayer.ChampionStats.BonusArmorPenetrationPercent * 100f;
        public float BonusMagicPenetrationPercent => RootGameData.ActivePlayer.ChampionStats.BonusMagicPenetrationPercent * 100f;
        public float CooldownReduction => RootGameData.ActivePlayer.ChampionStats.CooldownReduction * -100f;//40% cdr comes from the game as -0.4
        public float CritChance => RootGameData.ActivePlayer.ChampionStats.CritChance * 100f;
        public float CritDamagePercent => RootGameData.ActivePlayer.ChampionStats.CritDamage;
        public float HealthCurrent => RootGameData.ActivePlayer.ChampionStats.CurrentHealth;
        public float HealthMax => RootGameData.ActivePlayer.ChampionStats.MaxHealth;
        public float HealthRegenRate => RootGameData.ActivePlayer.ChampionStats.HealthRegenRate;
        public float LifeSteal => RootGameData.ActivePlayer.ChampionStats.LifeSteal * 100f;
        public float MagicLethality => RootGameData.ActivePlayer.ChampionStats.MagicLethality;
        public float MagicPenetrationFlat => RootGameData.ActivePlayer.ChampionStats.MagicPenetrationFlat;
        public float MagicPenetrationPercent => RootGameData.ActivePlayer.ChampionStats.MagicPenetrationPercent * 100f;
        public float MagicResist => RootGameData.ActivePlayer.ChampionStats.MagicResist;
        public float MoveSpeed => RootGameData.ActivePlayer.ChampionStats.MoveSpeed;
        public float PhysicalLethality => RootGameData.ActivePlayer.ChampionStats.PhysicalLethality;
        public float ResourceCurrent => RootGameData.ActivePlayer.ChampionStats.ResourceValue;
        public float ResourceMax => RootGameData.ActivePlayer.ChampionStats.ResourceMax;
        public float ResourceRegenRate => RootGameData.ActivePlayer.ChampionStats.ResourceRegenRate;
        public ResourceType ResourceType => ParseEnum<ResourceType>.TryParseOr(RootGameData.ActivePlayer.ChampionStats.ResourceType, ResourceType.Unknown);
        public float SpellVamp => RootGameData.ActivePlayer.ChampionStats.SpellVamp * 100f;
        public float Tenacity => RootGameData.ActivePlayer.ChampionStats.Tenacity * 100f;
    }
}
