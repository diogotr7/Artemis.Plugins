using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceChannelDataModel : DataModel
    {
        public string Id { get; set; }
        public string GuildId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Bitrate { get; set; }
        public int UserLimit { get; set; }
        public int Position { get; set; }
    }
}