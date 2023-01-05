using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordUserDataModel : DataModel
{
    public string Username { get; set; } = string.Empty;

    public string Discriminator { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public bool IsBot { get; set; }

    internal void Apply(User user)
    {
        Username = user.Username;
        Discriminator = user.Discriminator;
        Id = user.Id;
        IsBot = user.Bot ?? false;
    }
}