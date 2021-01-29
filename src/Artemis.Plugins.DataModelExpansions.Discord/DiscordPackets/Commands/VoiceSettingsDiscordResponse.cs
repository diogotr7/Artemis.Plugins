using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public record VoiceSettingsData
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
    ) : DiscordMessageData;

    public record Shortcut
    (
         //0 - KEYBOARD_KEY
         //1 - MOUSE_BUTTON
         //2 - KEYBOARD_MODIFIER_KEY
         //3 - GAMEPAD_BUTTON
         int Type,
         int Code,
         string Name
    );

    public record VoiceSettingsMode
    (
        string Type,
        bool AutoThreshold,
        float Threshold,
        Shortcut[] Shortcut,
        float Delay
    );

    public record AudioDevice
    (
        string Id,
        string Name
    );

    public record InputOutput
    (
        List<AudioDevice> AvailableDevices,
        string DeviceId,
        float Volume
    );
}