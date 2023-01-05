using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordShortcut : DataModel
{
    public DiscordKeyType Type { get; set; }
    public int Code { get; set; }
    public string? Name { get; set; }
}