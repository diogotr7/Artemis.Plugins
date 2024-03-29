using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.Modules.Spotify;

public partial class SpotifyConfigurationDialogView : ReactiveUserControl<SpotifyConfigurationDialogViewModel>
{
    public SpotifyConfigurationDialogView()
    {
        InitializeComponent();
    }
}
