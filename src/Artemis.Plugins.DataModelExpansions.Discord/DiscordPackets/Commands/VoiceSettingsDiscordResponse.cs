using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class VoiceSettingsDiscordResponse : DiscordResponse
    {
        public VoiceSettingsData Data { get; set; }
    }

    public class VoiceSettingsMode
    {
        public string Type { get; set; }
        public bool AutoThreshold { get; set; }
        public float Threshold { get; set; }
        //shortcut???
        public float Delay { get; set; }
    }

    public class AudioDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class InputOutput
    {
        public List<AudioDevice> AvailableDevices { get; set; }

        public string DeviceId { get; set; }

        public float Volume { get; set; }
    }

    public class VoiceSettingsData
    {
        public bool AutomaticGainControl { get; set; }
        public bool EchoCancellation { get; set; }
        public bool NoiseSuppression { get; set; }
        public bool Qos { get; set; }
        public bool SilenceWarning { get; set; }
        public bool Deaf { get; set; }
        public bool Mute { get; set; }
        public VoiceSettingsMode Mode { get; set; }
        public InputOutput Input { get; set; }
        public InputOutput Output { get; set; }
    }
}