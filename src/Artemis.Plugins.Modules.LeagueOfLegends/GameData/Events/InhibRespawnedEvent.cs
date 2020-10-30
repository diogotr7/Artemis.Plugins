using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibRespawnedEvent : LolEvent
    {
        public string InhibRespawned { get; set; }
    }

    public class InhibRespawnedEventArgs : DataModelEventArgs
    {
        public string InhibRespawned { get; set; }
    }
}
