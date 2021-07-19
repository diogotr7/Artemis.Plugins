using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs
{
    public class InhibRespawningSoonEventArgs : DataModelEventArgs
    {
        public Inhibitor InhibRespawningSoon { get; set; }
    }
}
