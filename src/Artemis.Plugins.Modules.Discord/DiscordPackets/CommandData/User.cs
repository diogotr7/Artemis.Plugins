namespace Artemis.Plugins.Modules.Discord
{
    public record User
    (
        string Username,
        string Discriminator,
        string Id,
        string Avatar,
        bool? Bot
    );
}
