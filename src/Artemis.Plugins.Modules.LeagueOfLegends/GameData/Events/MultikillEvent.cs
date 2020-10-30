using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class MultikillEvent : LolEvent
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }

    public class MultikillEventArgs : DataModelEventArgs
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }
}
