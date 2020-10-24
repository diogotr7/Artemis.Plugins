using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using RGB.NET.Devices.OpenRGB;
using System.Collections.Generic;

namespace Artemis.Plugins.Devices.OpenRGB
{
    public class OpenRGBDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;
        private readonly PluginSettings _settings;

        public OpenRGBDeviceProvider(IRgbService rgbService, PluginSettings settings) : base(RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance)
        {
            _rgbService = rgbService;
            _settings = settings;
        }

        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<OpenRGBConfigurationDialogViewModel>();

            PluginSetting<List<OpenRGBServerDefinition>> definitions = _settings.GetSetting<List<OpenRGBServerDefinition>>("DeviceDefinitions");
            if (definitions.Value is null)
            {
                definitions.Value = new List<OpenRGBServerDefinition>();
                OpenRGBServerDefinition definition = new OpenRGBServerDefinition
                {
                    ClientName = "Artemis",
                    Ip = "127.0.0.1",
                    Port = 6742
                };
                definitions.Value.Add(definition);
                definitions.Save();
            }

            foreach (OpenRGBServerDefinition def in definitions.Value)
            {
                RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance.DeviceDefinitions.Add(def);
            }

            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}