using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class AceEvent : LolEvent
    {
        public string Acer { get; set; }
        public string AcingTeam { get; set; }
    }

    public class AceEventArgs : DataModelEventArgs
    {
        public string Acer { get; set; }
        public string AcingTeam { get; set; }
    }
}
