using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Spotify;

public class SpotifySettingsWindowViewModel : ActivatableViewModelBase
{
    public SpotifySettingsWindowViewModel(SpotifyConfigurationDialogViewModel configurationViewModel)
    {
        ConfigurationViewModel = configurationViewModel;
    }

    public SpotifyConfigurationDialogViewModel ConfigurationViewModel { get; }
}
