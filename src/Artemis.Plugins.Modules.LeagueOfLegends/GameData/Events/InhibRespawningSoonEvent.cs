using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibRespawningSoonEvent : LolEvent
    {
        public string InhibRespawningSoon { get; set; }
    }

    public class InhibRespawningSoonEventArgs : DataModelEventArgs
    {
        public Inhibitor InhibRespawningSoon { get; set; }
    }
}
