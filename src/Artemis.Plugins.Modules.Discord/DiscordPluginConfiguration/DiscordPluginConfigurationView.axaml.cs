using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.Plugins.Modules.Discord
{
    public partial class DiscordPluginConfigurationView : UserControl
    {
        public DiscordPluginConfigurationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
