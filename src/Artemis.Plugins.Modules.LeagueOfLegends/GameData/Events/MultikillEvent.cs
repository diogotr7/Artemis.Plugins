namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class MultikillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }
}
