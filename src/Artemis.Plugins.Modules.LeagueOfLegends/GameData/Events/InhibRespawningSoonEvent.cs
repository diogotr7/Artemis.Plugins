using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibRespawningSoonEvent : LolEvent
    {
        public string InhibRespawningSoon { get; set; }
    }

    public class InhibRespawningSoonEventArgs : DataModelEventArgs
    {
        public string InhibRespawningSoon { get; set; }
    }
}
