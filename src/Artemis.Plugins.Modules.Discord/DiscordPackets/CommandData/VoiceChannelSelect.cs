namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record VoiceChannelSelect
(
    string? GuildId,
    string? ChannelId
);