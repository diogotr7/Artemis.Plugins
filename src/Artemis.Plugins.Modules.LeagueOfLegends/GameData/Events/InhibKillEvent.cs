using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class InhibKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class InhibKillEventArgs : DataModelEventArgs
    {
        public string KillerName { get; set; }
        public Inhibitor InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }
}
