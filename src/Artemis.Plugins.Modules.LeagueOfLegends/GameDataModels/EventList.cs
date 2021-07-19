using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class EventList
    {
        public LolEvent[] Events { get; set; } = Array.Empty<LolEvent>();
    }
}
