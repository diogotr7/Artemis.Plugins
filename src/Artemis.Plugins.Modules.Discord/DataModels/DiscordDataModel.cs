using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordDataModel : DataModel
    {
        public DiscordUserDataModel User { get; set; } = new DiscordUserDataModel();

        public DiscordVoiceSettingsDataModel VoiceSettings { get; set; } = new DiscordVoiceSettingsDataModel();

        public DiscordVoiceConnectionStatusDataModel VoiceConnection { get; set; } = new DiscordVoiceConnectionStatusDataModel();

        public DataModelEvent<DiscordNotificationEventArgs> Notification { get; set; } = new DataModelEvent<DiscordNotificationEventArgs>();
    }

    public class DiscordNotificationEventArgs : DataModelEventArgs
    {
        public string ChannelId { get; set; }
        public User Author { get; set; }
        public string IconUrl { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

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

    public enum DiscordVoiceModeType
    {
        PUSH_TO_TALK,
        VOICE_ACTIVITY
    }

    public class DiscordVoiceMode : DataModel
    {
        public DiscordVoiceModeType Type { get; set; }
        public bool AutoThreshold { get; set; }
        public float Threshold { get; set; }
        public DiscordShortcut[] Shortcut { get; set; } = System.Array.Empty<DiscordShortcut>();
        public float Delay { get; set; }
    }

    public enum DiscordKeyType : int
    {
        KEYBOARD_KEY,
        MOUSE_BUTTON,
        KEYBOARD_MODIFIER_KEY,
        GAMEPAD_BUTTON
    }

    public class DiscordShortcut : DataModel
    {
        public DiscordKeyType Type { get; set; }
        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class DiscordUserDataModel : DataModel
    {
        public string Username { get; set; }

        public string Discriminator { get; set; }

        public string Id { get; set; }
    }

    public class DiscordVoiceConnectionStatusDataModel : DataModel
    {
        public string State { get; set; }
        public string Hostname { get; set; }
        public float? Ping { get; set; }
        public bool Speaking { get; set; }
        public DataModelEvent Connected { get; set; } = new DataModelEvent();
        public DataModelEvent Disconnected { get; set; } = new DataModelEvent();
    }
}