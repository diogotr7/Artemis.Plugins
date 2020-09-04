using System.Collections.Generic;
using RGB.NET.Core;

namespace RgbNetDeviceProvider
{
    // This is your RGB.NET device provider
    // See https://github.com/DarthAffe/RGB.NET for more info on RGB.NET and examples of other device providers
    public class MyPluginDeviceProvider : IRGBDeviceProvider
    {
        private static MyPluginDeviceProvider _instance;
        public static MyPluginDeviceProvider Instance => _instance ?? new MyPluginDeviceProvider();

        // You may remove this if not applicable for your device
        public static List<string> PossibleX86NativePaths { get; } = new List<string> { "x86/MyNativeSDK.dll" };
        public static List<string> PossibleX64NativePaths { get; } = new List<string> { "x64/MyNativeSDK.dll" };

        public bool IsInitialized { get; private set; }
        public IEnumerable<IRGBDevice> Devices { get; private set; }
        public bool HasExclusiveAccess { get; private set; }

        public bool Initialize(RGBDeviceType loadFilter = RGBDeviceType.All, bool exclusiveAccessIfPossible = false, bool throwExceptions = false)
        {
            try
            {
                // Initialize your device here
            }
            catch
            {
                if (throwExceptions) throw;
                return false;
            }

            return true;
        }

        public void Dispose()
        {
        }

        public void ResetDevices()
        {
        }
    }
}
