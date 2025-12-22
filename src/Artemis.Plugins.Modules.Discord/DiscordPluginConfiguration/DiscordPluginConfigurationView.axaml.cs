using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;

namespace Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;

public partial class DiscordPluginConfigurationView : ReactiveUserControl<DiscordPluginConfigurationViewModel>
{
    public DiscordPluginConfigurationView()
    {
        InitializeComponent();
    }
}
