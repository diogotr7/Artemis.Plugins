namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record VoiceState(
    bool Mute,
    bool Deaf,
    bool SelfMute,
    bool SelfDeaf,
    bool Suppress
);