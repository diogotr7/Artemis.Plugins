namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record User
(
    string Username,
    string Discriminator,
    string Id,
    string Avatar,
    bool? Bot
);
