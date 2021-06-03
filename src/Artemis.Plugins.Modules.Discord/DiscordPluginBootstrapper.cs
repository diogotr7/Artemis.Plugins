using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            plugin.ConfigurationDialog = new PluginConfigurationDialog<DiscordPluginConfigurationViewModel>();
        }
    }
}
