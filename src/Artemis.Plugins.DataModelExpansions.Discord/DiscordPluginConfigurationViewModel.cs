using Artemis.Core;
using Artemis.UI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class DiscordPluginConfigurationViewModel : PluginConfigurationViewModel
    {
        private string _clientId;
        private string _clientSecret;
        private readonly PluginSetting<string> _clientIdSetting;
        private readonly PluginSetting<string> _clientSecretSetting;

        public string ClientSecret
        {
            get => _clientSecret;
            set => SetAndNotify(ref _clientSecret, value);
        }
        public string ClientId
        {
            get => _clientId;
            set => SetAndNotify(ref _clientId, value);
        }

        public DiscordPluginConfigurationViewModel(Plugin plugin, PluginSettings pluginSettings) : base(plugin)
        {
            _clientIdSetting = pluginSettings.GetSetting<string>("DiscordClientId", null);
            _clientSecretSetting = pluginSettings.GetSetting<string>("DiscordClientSecret", null);

            _clientId = _clientIdSetting.Value;
            _clientSecret = _clientSecretSetting.Value;
        }

        public void Save()
        {
            _clientIdSetting.Value = _clientId;
            _clientIdSetting.Save();
            _clientSecretSetting.Value = _clientSecret;
            _clientSecretSetting.Save();
        }
    }
}
