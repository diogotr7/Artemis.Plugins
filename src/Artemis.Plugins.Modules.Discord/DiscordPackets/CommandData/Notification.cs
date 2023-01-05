namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record Notification
(
    string ChannelId,
    Message Message,
    string IconUrl,
    string Title,
    string Body
);