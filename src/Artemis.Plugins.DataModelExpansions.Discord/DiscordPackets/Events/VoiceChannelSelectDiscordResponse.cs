using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    internal class VoiceChannelSelectDiscordResponse : EventDiscordResponse
    {
        public VoiceChannelSelectData Data { get; set; }
    }

    public class VoiceChannelSelectData
    {
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
    }
}