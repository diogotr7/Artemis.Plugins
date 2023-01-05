namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record VoiceSettingsMode
(
    string Type,
    bool AutoThreshold,
    float Threshold,
    Shortcut[] Shortcut,
    float Delay
);