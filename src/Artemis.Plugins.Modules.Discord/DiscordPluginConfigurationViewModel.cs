using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordPluginConfigurationViewModel : PluginConfigurationViewModel
    {
        private string _clientId;
        private string _clientSecret;
        private readonly PluginSetting<string> _clientIdSetting;
        private readonly PluginSetting<string> _clientSecretSetting;
        private readonly PluginSetting<SavedToken> _tokenSetting;

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
            _tokenSetting = pluginSettings.GetSetting<SavedToken>("DiscordToken", null);

            _clientId = _clientIdSetting.Value;
            _clientSecret = _clientSecretSetting.Value;
        }

        public void Save()
        {
            _clientIdSetting.Value = _clientId.Trim();
            _clientIdSetting.Save();
            _clientSecretSetting.Value = _clientSecret.Trim();
            _clientSecretSetting.Save();
        }

        public void Reset()
        {
            ClientSecret = null;
            ClientId = null;

            _clientSecretSetting.Value = null;
            _clientSecretSetting.Save();

            _clientIdSetting.Value = null;
            _clientIdSetting.Save();

            _tokenSetting.Value = null;
            _tokenSetting.Save();
        }
    }
}
