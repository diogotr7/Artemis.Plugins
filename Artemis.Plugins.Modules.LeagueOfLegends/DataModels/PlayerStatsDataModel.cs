using Artemis.Core.DataModelExpansions;

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
    }

    public enum ResourceType
    {
        Unknown = -1,
        None = 0,
        Mana,
        Energy,
        Shield,
        Battlefury,
        Dragonfury,
        Rage,
        Heat,
        Gnarfury,
        Ferocity,
        Bloodwell,
        Wind,
        Ammo,
        Other,
        Max
    }
}
