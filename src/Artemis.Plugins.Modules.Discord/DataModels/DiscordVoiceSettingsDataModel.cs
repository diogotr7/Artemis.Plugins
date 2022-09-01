using Artemis.Core.Modules;
using System;
using System.Linq;

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

        internal void Apply(VoiceSettings voiceData)
        {
            AutomaticGainControl = voiceData.AutomaticGainControl;
            EchoCancellation = voiceData.EchoCancellation;
            NoiseSuppression = voiceData.NoiseSuppression;
            Qos = voiceData.Qos;
            SilenceWarning = voiceData.SilenceWarning;
            Deafened = voiceData.Deaf;
            Muted = voiceData.Mute;
            Mode.Type = Enum.Parse<DiscordVoiceModeType>(voiceData.Mode.Type);
            Mode.AutoThreshold = voiceData.Mode.AutoThreshold;
            Mode.Threshold = voiceData.Mode.Threshold;
            Mode.Shortcut = voiceData.Mode.Shortcut
                .Select(ds => new DiscordShortcut
                {
                    Type = (DiscordKeyType)ds.Type,
                    Code = ds.Code,
                    Name = ds.Name
                })
                .ToArray();
        }
    }
}