﻿using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System.Reactive;

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
            PluginSettings pluginSettings)
            : base(plugin)
        {
            _clientIdSetting = pluginSettings.GetSetting("DiscordClientId", string.Empty);
            _clientSecretSetting = pluginSettings.GetSetting("DiscordClientSecret", string.Empty);

            ClientId = _clientIdSetting.Value;
            ClientSecret = _clientSecretSetting.Value;

            this.ValidationRule(vm => vm.ClientId, clientid => clientid?.Length == 18, "Client Id must be 18 characters long");
            this.ValidationRule(vm => vm.ClientSecret, clientSecret => clientSecret?.Length == 32, "Client Secret must be 32 characters long");

            Save = ReactiveCommand.Create(ExecuteSave, ValidationContext.Valid);
        }

        public string ClientId
        {
            get => _clientId;
            set => RaiseAndSetIfChanged(ref _clientId, value);
        }

        public string ClientSecret
        {
            get => _clientSecret;
            set => RaiseAndSetIfChanged(ref _clientSecret, value);
        }

        public ReactiveCommand<Unit, Unit> Save { get; }

        public void ExecuteSave()
        {
            _clientIdSetting.Value = ClientId;
            _clientIdSetting.Save();

            _clientSecretSetting.Value = ClientSecret;
            _clientSecretSetting.Save();
        }
    }
}
