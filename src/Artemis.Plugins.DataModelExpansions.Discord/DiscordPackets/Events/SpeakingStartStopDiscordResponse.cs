namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public record SpeakingStartData(string UserId) : DiscordMessageData;
    public record SpeakingStopData(string UserId) : DiscordMessageData;
}