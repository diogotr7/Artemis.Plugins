using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record InputOutput
    (
        List<AudioDevice> AvailableDevices,
        string DeviceId,
        float Volume
    );
}