using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class VoiceConnectionStatusDiscordEvent : DiscordEvent
    {
        public VoiceConnectionStatusData Data { get; set; }
    }

    public class VoiceConnectionStatusData
    {
        public string State { get; set; }

        public string Hostname { get; set; }

        public List<int> Pings { get; set; }

        public float AveragePing { get; set; }

        public float? LastPing { get; set; }
    }
}