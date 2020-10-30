namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class ChampionKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string VictimName { get; set; }
        public string[] Assisters { get; set; }
    }
}
