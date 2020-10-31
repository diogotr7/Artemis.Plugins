using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class TurretKillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public string TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class TurretKillEventArgs : DataModelEventArgs
    {
        public string KillerName { get; set; }
        public Turret TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }
}
