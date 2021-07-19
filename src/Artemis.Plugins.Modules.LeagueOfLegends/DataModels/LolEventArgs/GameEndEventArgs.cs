using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs
{
    public class GameEndEventArgs : DataModelEventArgs
    {
        public bool Win { get; set; }
    }
}
