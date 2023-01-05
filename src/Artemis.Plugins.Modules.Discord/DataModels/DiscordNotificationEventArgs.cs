using Artemis.Core;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordNotificationEventArgs : DataModelEventArgs
{
    public string ChannelId { get; set; } = string.Empty;
    public User? Author { get; set; } = default!;
    public string IconUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}