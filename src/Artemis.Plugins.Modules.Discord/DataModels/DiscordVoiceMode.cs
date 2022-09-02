using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceMode : DataModel
    {
        public DiscordVoiceModeType Type { get; set; }
        public bool AutoThreshold { get; set; }
        public float Threshold { get; set; }
        public DiscordShortcut[] Shortcut { get; set; } = System.Array.Empty<DiscordShortcut>();
        public float Delay { get; set; }
    }
}