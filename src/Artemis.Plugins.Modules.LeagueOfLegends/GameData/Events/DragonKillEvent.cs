namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class DragonKillEvent : LolEvent
    {
        public string DragonType { get; set; }
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }
}
