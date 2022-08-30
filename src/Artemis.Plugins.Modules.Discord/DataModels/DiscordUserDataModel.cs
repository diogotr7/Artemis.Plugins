using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordUserDataModel : DataModel
    {
        public string Username { get; set; }

        public string Discriminator { get; set; }

        public string Id { get; set; }
    }
}