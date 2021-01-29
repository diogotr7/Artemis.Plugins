namespace Artemis.Plugins.DataModelExpansions.Discord
{
    internal class VoiceChannelSelectDiscordEvent : DiscordEvent
    {
        public VoiceChannelSelectData Data { get; set; }
    }

    public record VoiceChannelSelectData
    (
        string GuildId,
        string ChannelId
    );
}