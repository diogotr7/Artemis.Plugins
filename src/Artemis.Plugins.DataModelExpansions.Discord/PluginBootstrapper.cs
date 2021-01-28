using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class PluginBootstrapper : IPluginBootstrapper
    {
        public void Disable(Plugin plugin)
        {
        }

        public void Enable(Plugin plugin)
        {
            plugin.ConfigurationDialog = new PluginConfigurationDialog<DiscordPluginConfigurationViewModel>();
        }
    }
}
