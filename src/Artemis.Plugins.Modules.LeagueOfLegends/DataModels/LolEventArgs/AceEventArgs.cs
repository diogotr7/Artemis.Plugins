using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs
{
    public class AceEventArgs : DataModelEventArgs
    {
        public string Acer { get; set; }
        public Team AcingTeam { get; set; }
    }
}
