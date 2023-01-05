using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordUserDataModel : DataModel
{
    public string Username { get; set; }

    public string Discriminator { get; set; }

    public string Id { get; set; }

    public bool IsBot { get; set; }

    internal void Apply(User user)
    {
        Username = user.Username;
        Discriminator = user.Discriminator;
        Id = user.Id;
        IsBot = user.Bot ?? false;
    }
}