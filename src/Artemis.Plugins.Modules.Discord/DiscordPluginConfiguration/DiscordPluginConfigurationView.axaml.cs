using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;

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
