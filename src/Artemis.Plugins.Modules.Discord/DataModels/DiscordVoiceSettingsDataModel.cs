using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceSettingsDataModel : DataModel
    {
        public bool Muted { get; set; }
        public bool Deafened { get; set; }
        public bool AutomaticGainControl { get; set; }
        public bool EchoCancellation { get; set; }
        public bool NoiseSuppression { get; set; }
        public bool Qos { get; set; }
        public bool SilenceWarning { get; set; }
        public DiscordVoiceMode Mode { get; set; } = new DiscordVoiceMode();
    }
}