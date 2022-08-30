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
}