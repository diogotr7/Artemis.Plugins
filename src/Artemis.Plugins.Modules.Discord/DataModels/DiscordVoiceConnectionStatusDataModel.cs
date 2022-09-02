using Artemis.Core;
using Artemis.Core.Modules;
using System;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceConnectionStatusDataModel : DataModel
    {
        public DiscordVoiceChannelState State { get; set; }

        public string Hostname { get; set; }

        public float? Ping { get; set; }

        public bool IsConnected => State == DiscordVoiceChannelState.VOICE_CONNECTED;

        internal void Apply(VoiceConnectionStatus e)
        {
            State = Enum.Parse<DiscordVoiceChannelState>(e.State);
            Ping = e.LastPing;
            Hostname = e.Hostname;
        }
    }
}