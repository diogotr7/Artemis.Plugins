namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }
}
