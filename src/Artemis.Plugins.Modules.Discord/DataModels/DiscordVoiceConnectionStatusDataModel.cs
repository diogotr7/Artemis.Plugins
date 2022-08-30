using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceConnectionStatusDataModel : DataModel
    {
        public DiscordVoiceChannelState State { get; set; }
        public string Hostname { get; set; }
        public float? Ping { get; set; }
        public bool Speaking { get; set; }
        public bool IsVoiceConnected => State == DiscordVoiceChannelState.VOICE_CONNECTED;
        public DataModelEvent Connected { get; set; } = new DataModelEvent();
        public DataModelEvent Disconnected { get; set; } = new DataModelEvent();
    }
}