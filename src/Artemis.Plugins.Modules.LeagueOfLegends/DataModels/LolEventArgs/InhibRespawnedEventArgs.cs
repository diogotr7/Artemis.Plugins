using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs
{
    public class InhibRespawnedEventArgs : DataModelEventArgs
    {
        public Inhibitor InhibRespawned { get; set; }
    }
}
