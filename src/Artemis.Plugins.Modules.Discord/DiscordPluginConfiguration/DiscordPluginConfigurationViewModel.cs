using System;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;

public class DiscordPluginConfigurationViewModel : PluginConfigurationViewModel
{
    private readonly PluginSettings _pluginSettings;
    private readonly Plugin _plugin;
    
    public DiscordPluginConfigurationViewModel(Plugin plugin, PluginSettings pluginSettings) : base(plugin)
    {
        _pluginSettings = pluginSettings;
        _plugin = plugin;
        
        this.WhenActivated(d =>
        {
            Disposable.Create(() =>
            {
                Task.Run(async () =>
                {
                    _pluginSettings.SaveAllSettings();

                    var feature = _plugin.GetFeature<DiscordModule>();
                    
                    feature?.DisconnectFromDiscord();
                    await Task.Delay(1000);
                    feature?.ConnectToDiscord();
                });
            }).DisposeWith(d);
        });
    }
    
    public PluginSetting<string> ClientIdSetting => _pluginSettings.GetSetting("DiscordClientId", string.Empty);
    public PluginSetting<string> ClientSecretSetting => _pluginSettings.GetSetting("DiscordClientSecret", string.Empty);
    public PluginSetting<DiscordRpcProvider> Provider => _pluginSettings.GetSetting("DiscordRpcProvider", DiscordRpcProvider.StreamKit);
}
