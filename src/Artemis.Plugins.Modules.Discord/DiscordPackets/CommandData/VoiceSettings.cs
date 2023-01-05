namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record VoiceSettings
(
    bool AutomaticGainControl,
    bool EchoCancellation,
    bool NoiseSuppression,
    bool Qos,
    bool SilenceWarning,
    bool Deaf,
    bool Mute,
    VoiceSettingsMode Mode,
    InputOutput Input,
    InputOutput Output
);