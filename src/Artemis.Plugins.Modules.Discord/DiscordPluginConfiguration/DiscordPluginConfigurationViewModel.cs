using Artemis.Core;
using Artemis.UI.Shared;
using Stylet;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordPluginConfigurationViewModel : PluginConfigurationViewModel
    {
        private readonly PluginSetting<string> _clientIdSetting;
        private readonly PluginSetting<string> _clientSecretSetting;
        private string _clientId;
        private string _clientSecret;

        public DiscordPluginConfigurationViewModel(
            Plugin plugin,
            PluginSettings pluginSettings,
            IModelValidator<DiscordPluginConfigurationViewModel> validator)
            : base(plugin, validator)
        {
            _clientIdSetting = pluginSettings.GetSetting<string>("DiscordClientId", null);
            _clientSecretSetting = pluginSettings.GetSetting<string>("DiscordClientSecret", null);

            ClientId = _clientIdSetting.Value;
            ClientSecret = _clientSecretSetting.Value;
        }

        public string ClientId
        {
            get => _clientId;
            set => SetAndNotify(ref _clientId, value);
        }

        public string ClientSecret
        {
            get => _clientSecret;
            set => SetAndNotify(ref _clientSecret, value);
        }

        public void Save()
        {
            if (!Validate())
                return;

            _clientIdSetting.Value = _clientId.Trim();
            _clientIdSetting.Save();

            _clientSecretSetting.Value = _clientSecret.Trim();
            _clientSecretSetting.Save();
        }

        public void OpenWikiLink()
        {
            Utilities.OpenUrl("https://wiki.artemis-rgb.com/en/guides/user/plugins/discord");
        }
    }
}
