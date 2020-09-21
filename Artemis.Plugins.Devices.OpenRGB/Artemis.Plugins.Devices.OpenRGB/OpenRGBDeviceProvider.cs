using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;

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

            var ip = _settings.GetSetting("IpAddress", "127.0.0.1");
            var port = _settings.GetSetting("Port", 6742);

            RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance.ClientName = "Artemis";
            RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance.IpAddress = ip.Value;
            RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance.Port = port.Value;

            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}