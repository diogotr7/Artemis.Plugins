using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class RootGameData
    {
        public ActivePlayer ActivePlayer { get; set; } = new();
        public AllPlayer[] AllPlayers { get; set; } = Array.Empty<AllPlayer>();
        public EventList Events { get; set; } = new();
        public GameStats GameData { get; set; } = new();
    }
}
