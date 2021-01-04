using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    internal class SpeakingStartDiscordEvent : DiscordEvent
    {
        public SpeakingStartStopData Data { get; set; }
    }

    internal class SpeakingStopDiscordEvent : DiscordEvent
    {
        public SpeakingStartStopData Data { get; set; }
    }

    public class SpeakingStartStopData
    {
        public string UserId { get; set; }
    }
}