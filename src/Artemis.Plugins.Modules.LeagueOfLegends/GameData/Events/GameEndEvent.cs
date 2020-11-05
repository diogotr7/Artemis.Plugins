using Artemis.Core;
using Artemis.Core.DefaultTypes;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class GameEndEvent : LolEvent
    {
        public string Result { get; set; }
    }

    public class GameEndEventArgs : DataModelEventArgs
    {
        public bool Win { get; set; }
    }
}
