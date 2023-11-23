using Artemis.Core;
using Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Discord;

public class DiscordPluginBootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        //New version works without any configuration, so we don't need this
        //plugin.ConfigurationDialog = new PluginConfigurationDialog<DiscordPluginConfigurationViewModel>();
    }
}
