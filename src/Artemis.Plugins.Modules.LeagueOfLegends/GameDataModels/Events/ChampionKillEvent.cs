namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class ChampionKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string VictimName { get; set; }
        public string[] Assisters { get; set; }
    }
}
