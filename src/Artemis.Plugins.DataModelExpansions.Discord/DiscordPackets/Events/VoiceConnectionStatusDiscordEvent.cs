using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class VoiceConnectionStatusDiscordEvent : DiscordEvent
    {
        public VoiceConnectionStatusData Data { get; set; }
    }

    public record VoiceConnectionStatusData
    (
        string State,
        string Hostname,
        List<int> Pings,
        float AveragePing,
        float? LastPing
    );
}