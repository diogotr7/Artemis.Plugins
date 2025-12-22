using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;

namespace Artemis.Plugins.Modules.Spotify;

public partial class SpotifyConfigurationDialogView : ReactiveUserControl<SpotifyConfigurationDialogViewModel>
{
    public SpotifyConfigurationDialogView()
    {
        InitializeComponent();
    }
}
