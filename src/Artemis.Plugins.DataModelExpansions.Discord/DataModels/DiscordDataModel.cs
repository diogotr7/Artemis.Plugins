using Artemis.Core;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.Discord.DataModels
{
    public class DiscordDataModel : DataModel
    {
        public DiscordUserDataModel User { get; set; } = new DiscordUserDataModel();

        public DiscordVoiceSettingsDataModel VoiceSettings { get; set; } = new DiscordVoiceSettingsDataModel();

        public DiscordVoiceConnectionStatusDataModel VoiceConnection { get; set; } = new DiscordVoiceConnectionStatusDataModel();

        public DataModelEvent Notification { get; set; } = new DataModelEvent();
    }

    public class DiscordVoiceSettingsDataModel : DataModel
    {
        public bool Muted { get; set; }

        public bool Deafened { get; set; }

        public bool Speaking { get; set; }
    }

    public class DiscordUserDataModel : DataModel
    {
        public string Username { get; set; }

        public string Discriminator { get; set; }

        public string Id { get; set; }
    }

    public class DiscordVoiceConnectionStatusDataModel : DataModel
    {
        public float? Ping { get; set; }

        public string State { get; set; }

        public DataModelEvent Connected { get; set; } = new DataModelEvent();

        public DataModelEvent Disconnected { get; set; } = new DataModelEvent();
    }
}