using System;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;

public class DiscordPluginConfigurationViewModel : PluginConfigurationViewModel
{
    private readonly PluginSettings _pluginSettings;

    public DiscordPluginConfigurationViewModel(Plugin plugin, PluginSettings pluginSettings) : base(plugin)
    {
        _pluginSettings = pluginSettings;
        
        this.WhenActivated(d =>
        {
            Disposable.Create(() =>
            {
                _pluginSettings.SaveAllSettings();
            }).DisposeWith(d);
        });
    }
    
    public PluginSetting<string> ClientIdSetting => _pluginSettings.GetSetting("DiscordClientId", string.Empty);
    public PluginSetting<string> ClientSecretSetting => _pluginSettings.GetSetting("DiscordClientSecret", string.Empty);
    public PluginSetting<DiscordRpcProvider> Provider => _pluginSettings.GetSetting("DiscordRpcProvider", DiscordRpcProvider.StreamKit);
}
