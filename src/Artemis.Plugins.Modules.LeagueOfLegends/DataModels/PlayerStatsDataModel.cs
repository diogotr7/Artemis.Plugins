using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.Utils;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerStatsDataModel : DataModel
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
        public float CritDamagePercent { get; set; }
        public float HealthCurrent { get; set; }
        public float HealthMax { get; set; }
        public float HealthRegenRate { get; set; }
        public float LifeSteal { get; set; }
        public float MagicLethality { get; set; }
        public float MagicPenetrationFlat { get; set; }
        public float MagicPenetrationPercent { get; set; }
        public float MagicResist { get; set; }
        public float MoveSpeed { get; set; }
        public float PhysicalLethality { get; set; }
        public float ResourceCurrent { get; set; }
        public float ResourceMax { get; set; }
        public float ResourceRegenRate { get; set; }
        public ResourceType ResourceType { get; set; }
        public float SpellVamp { get; set; }
        public float Tenacity { get; set; }

        public void Apply(ChampionStats championStats)
        {
            AbilityPower = championStats.AbilityPower;
            Armor = championStats.Armor;
            ArmorPenetrationFlat = championStats.ArmorPenetrationFlat;
            ArmorPenetrationPercent = championStats.ArmorPenetrationPercent * 100f;
            AttackDamage = championStats.AttackDamage;
            AttackRange = championStats.AttackRange;
            AttackSpeed = championStats.AttackSpeed;
            BonusArmorPenetrationPercent = championStats.BonusArmorPenetrationPercent * 100f;
            BonusMagicPenetrationPercent = championStats.BonusMagicPenetrationPercent * 100f;
            CooldownReduction = championStats.CooldownReduction * -100f;//40% cdr comes from the game as -0.4
            CritChance = championStats.CritChance * 100f;
            CritDamagePercent = championStats.CritDamage;
            HealthCurrent = championStats.CurrentHealth;
            HealthMax = championStats.MaxHealth;
            HealthRegenRate = championStats.HealthRegenRate;
            LifeSteal = championStats.LifeSteal * 100f;
            MagicLethality = championStats.MagicLethality;
            MagicPenetrationFlat = championStats.MagicPenetrationFlat;
            MagicPenetrationPercent = championStats.MagicPenetrationPercent * 100f;
            MagicResist = championStats.MagicResist;
            MoveSpeed = championStats.MoveSpeed;
            PhysicalLethality = championStats.PhysicalLethality;
            ResourceCurrent = championStats.ResourceValue;
            ResourceMax = championStats.ResourceMax;
            ResourceRegenRate = championStats.ResourceRegenRate;
            ResourceType = ParseEnum<ResourceType>.TryParseOr(championStats.ResourceType, ResourceType.Unknown);
            SpellVamp = championStats.SpellVamp * 100f;
            Tenacity = championStats.Tenacity * 100f;
        }
    }
}
