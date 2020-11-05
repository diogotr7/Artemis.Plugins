using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibRespawnedEvent : LolEvent
    {
        public string InhibRespawned { get; set; }
    }

    public class InhibRespawnedEventArgs : DataModelEventArgs
    {
        public Inhibitor InhibRespawned { get; set; }
    }
}
