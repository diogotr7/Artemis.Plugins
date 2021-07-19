namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class MultikillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }
}
