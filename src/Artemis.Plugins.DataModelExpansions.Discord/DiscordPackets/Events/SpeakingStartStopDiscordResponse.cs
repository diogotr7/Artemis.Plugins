using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    internal class SpeakingStartDiscordResponse : EventDiscordResponse
    {
        public SpeakingStartStopData Data { get; set; }
    }

    internal class SpeakingStopDiscordResponse : EventDiscordResponse
    {
        public SpeakingStartStopData Data { get; set; }
    }

    public class SpeakingStartStopData
    {
        public string UserId { get; set; }
    }
}