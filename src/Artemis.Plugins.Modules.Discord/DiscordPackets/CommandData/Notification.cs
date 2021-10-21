namespace Artemis.Plugins.Modules.Discord
{
    public record Notification
    (
        string ChannelId,
        Message Message,
        string IconUrl,
        string Title,
        string Body
    );
}