using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.Plugins.Modules.Spotify
{
    public partial class SpotifyConfigurationDialogView : UserControl
    {
        public SpotifyConfigurationDialogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
