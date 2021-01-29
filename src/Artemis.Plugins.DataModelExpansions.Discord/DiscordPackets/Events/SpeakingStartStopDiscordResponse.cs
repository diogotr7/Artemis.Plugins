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

    public record SpeakingStartStopData(string UserId);
}