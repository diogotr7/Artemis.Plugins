namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class RootGameData
    {
        public ActivePlayer ActivePlayer { get; set; }
        public AllPlayer[] AllPlayers { get; set; }
        public EventList Events { get; set; }
        public GameStats GameData { get; set; }
    }
}
