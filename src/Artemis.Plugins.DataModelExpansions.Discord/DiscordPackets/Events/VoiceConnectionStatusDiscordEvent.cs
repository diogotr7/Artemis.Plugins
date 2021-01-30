using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public record VoiceConnectionStatusData
    (
        string State,
        string Hostname,
        List<int> Pings,
        float AveragePing,
        float? LastPing
    );
}