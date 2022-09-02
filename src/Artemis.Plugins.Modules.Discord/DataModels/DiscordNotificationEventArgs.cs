using Artemis.Core;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordNotificationEventArgs : DataModelEventArgs
    {
        public string ChannelId { get; set; }
        public User Author { get; set; }
        public string IconUrl { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}