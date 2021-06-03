using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Spotify
{
    public class SpotifyPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            plugin.ConfigurationDialog = new PluginConfigurationDialog<SpotifyConfigurationDialogViewModel>();
        }
    }
}
