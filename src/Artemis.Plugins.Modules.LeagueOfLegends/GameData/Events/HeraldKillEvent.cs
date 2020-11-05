namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class HeraldKillEvent : LolEvent
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }
}
