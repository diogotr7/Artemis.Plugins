using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record VoiceConnectionStatus
    (
        string State,
        string Hostname,
        List<int> Pings,
        float AveragePing,
        float? LastPing
    );
}