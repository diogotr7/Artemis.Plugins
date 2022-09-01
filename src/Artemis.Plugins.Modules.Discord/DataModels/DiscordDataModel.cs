using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordDataModel : DataModel
    {
        public DiscordUserDataModel User { get; } = new();

        public DiscordVoiceDataModel Voice { get; } = new();

        public DataModelEvent<DiscordNotificationEventArgs> Notification { get;  } = new();
    }
}