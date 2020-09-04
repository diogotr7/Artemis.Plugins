using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;

namespace Artemis.Plugins.Devices.OpenRGB
{
    public class OpenRGBDeviceProvider : DeviceProvider
    {
        private readonly IRgbService _rgbService;

        public OpenRGBDeviceProvider(IRgbService rgbService) : base(RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.Instance)
        {
            _rgbService = rgbService;
        }

        public override void EnablePlugin()
        {
            RGB.NET.Devices.OpenRGB.OpenRGBDeviceProvider.ClientName = "Artemis";
            _rgbService.AddDeviceProvider(RgbDeviceProvider);
        }
    }
}