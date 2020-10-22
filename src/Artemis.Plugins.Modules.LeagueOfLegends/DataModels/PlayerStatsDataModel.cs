using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

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

        internal void Reset()
        {
            AbilityPower = -1;
            Armor = -1;
            ArmorPenetrationFlat = -1;
            ArmorPenetrationPercent = -1;
            AttackDamage = -1;
            AttackRange = -1;
            AttackSpeed = -1;
            BonusArmorPenetrationPercent = -1;
            BonusMagicPenetrationPercent = -1;
            CooldownReduction = -1;
            CritChance = -1;
            CritDamagePercent = -1;
            HealthCurrent = -1;
            HealthMax = -1;
            HealthRegenRate = -1;
            LifeSteal = -1;
            MagicLethality = -1;
            MagicPenetrationFlat = -1;
            MagicPenetrationPercent = -1;
            MagicResist = -1;
            MoveSpeed = -1;
            PhysicalLethality = -1;
            ResourceCurrent = -1;
            ResourceMax = -1;
            ResourceRegenRate = -1;
            ResourceType = ResourceType.None;
            SpellVamp = -1;
            Tenacity = -1;
        }
    }
}
