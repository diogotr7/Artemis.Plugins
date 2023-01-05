using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record InputOutput
(
    List<AudioDevice> AvailableDevices,
    string DeviceId,
    float Volume
);