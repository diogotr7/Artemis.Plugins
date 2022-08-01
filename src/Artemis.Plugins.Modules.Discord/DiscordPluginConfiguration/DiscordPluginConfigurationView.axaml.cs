using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.Modules.Discord
{
    public partial class DiscordPluginConfigurationView : ReactiveUserControl<DiscordPluginConfigurationViewModel>
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
