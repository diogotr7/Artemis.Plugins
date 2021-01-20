using Artemis.Core;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.Discord.DataModels
{
    public class DiscordDataModel : DataModel
    {
        [DataModelProperty]
        public DiscordUserDataModel User { get; set; } = new DiscordUserDataModel();

        [DataModelProperty]
        public DiscordVoiceSettingsDataModel VoiceSettings { get; set; } = new DiscordVoiceSettingsDataModel();

        [DataModelProperty]
        public DiscordVoiceConnectionStatusDataModel VoiceConnection { get; set; } = new DiscordVoiceConnectionStatusDataModel();

        [DataModelProperty]
        public DataModelEvent Notification { get; set; } = new DataModelEvent();
    }

    public class DiscordVoiceSettingsDataModel
    {
        [DataModelProperty]
        public bool Muted { get; set; }

        [DataModelProperty]
        public bool Deafened { get; set; }

        [DataModelProperty]
        public bool Speaking { get; set; }
    }

    public class DiscordUserDataModel
    {
        [DataModelProperty]
        public string Username { get; set; }

        [DataModelProperty]
        public string Discriminator { get; set; }

        [DataModelProperty]
        public string Id { get; set; }
    }

    public class DiscordVoiceConnectionStatusDataModel
    {
        [DataModelProperty]
        public float? Ping { get; set; }

        [DataModelProperty]
        public string State { get; set; }

        [DataModelProperty]
        public DataModelEvent Connected { get; set; }

        [DataModelProperty]
        public DataModelEvent Disconnected { get; set; }
    }
}