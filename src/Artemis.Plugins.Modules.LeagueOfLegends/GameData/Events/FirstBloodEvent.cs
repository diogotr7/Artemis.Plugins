using Artemis.Core;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameData
{
    public class FirstBloodEvent : LolEvent
    {
        public string Recipient { get; set; }
    }

    public class FirstBloodEventArgs : DataModelEventArgs
    {
        public string Recipient { get; set; }
    }
}
