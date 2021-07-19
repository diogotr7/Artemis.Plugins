namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class BaronKillEvent : LolEvent
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }
}
