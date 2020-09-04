using RGB.NET.Core;

namespace RgbNetDeviceProvider
{
    // This is your RGB.NET device provider loader
    // See https://github.com/DarthAffe/RGB.NET for more info on RGB.NET and examples of other device providers
    public class MyPluginDeviceProviderLoader : IRGBDeviceProviderLoader
    {
        // Set to true if you need some kind of initialization in your device provider
        public bool RequiresInitialization => false;
        public IRGBDeviceProvider GetDeviceProvider() => MyPluginDeviceProvider.Instance;
    }
}
