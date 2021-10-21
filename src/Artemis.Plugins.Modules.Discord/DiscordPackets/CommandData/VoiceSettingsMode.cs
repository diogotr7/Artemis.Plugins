namespace Artemis.Plugins.Modules.Discord
{
    public record VoiceSettingsMode
    (
        string Type,
        bool AutoThreshold,
        float Threshold,
        Shortcut[] Shortcut,
        float Delay
    );
}