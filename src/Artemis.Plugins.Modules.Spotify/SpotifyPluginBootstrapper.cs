using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Spotify;

public class SpotifyPluginBootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        plugin.ConfigurationDialog = new PluginConfigurationDialog<SpotifyConfigurationDialogViewModel>(true);
    }

    public override void OnPluginEnabled(Plugin plugin)
    {
        // Runs before the module is created, which depends on these
        plugin.Register<SpotifyAccount>();
        plugin.Register<AlbumArtColors>();
        plugin.Register<SpotifyLoginNotifier>();

        // Nothing depends on the notifier, resolve it so it starts listening
        plugin.Resolve<SpotifyLoginNotifier>();
    }
}
