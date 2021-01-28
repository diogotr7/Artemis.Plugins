namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums
{
    public enum ResourceType
    {
        [Name("UNKNOWN")] 
        Unknown = -1,
        [Name("NONE")] 
        None = 0,
        [Name("MANA")] 
        Mana,
        [Name("ENERGY")] 
        Energy,
        [Name("SHIELD")] 
        Shield,
        [Name("BATTLEFURY")] 
        Battlefury,
        [Name("DRAGONFURY")] 
        Dragonfury,
        [Name("RAGE")] 
        Rage,
        [Name("HEAT")] 
        Heat,
        [Name("GNARFURY")] 
        Gnarfury,
        [Name("FEROCITY")] 
        Ferocity,
        [Name("BLOODWELL")] 
        Bloodwell,
        [Name("WIND")] 
        Wind,
        [Name("AMMO")] 
        Ammo,
        [Name("OTHER")] 
        Other,
        [Name("MAX")] 
        Max
    }
}
