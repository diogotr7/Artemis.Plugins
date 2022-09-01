using Artemis.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.DataModels
{
    public class DiscordVoiceChannelMembers : DataModel { }
    
    public class DiscordVoiceChannelMember : DataModel
    {
        public DiscordUserDataModel User { get; } = new();
        public string Nickname { get; set; }
        public int Volume { get; set; }
        public bool IsServerMuted { get; set; }
        public bool IsSelfMuted { get; set; }
        public bool IsServerDeafened { get; set; }
        public bool IsSelfDeafened { get; set; }
        public bool IsSpeaking { get; set; }
        public bool IsSupressed { get; set; }
        public bool IsMutedByMe { get; set; }

        internal void Apply(UserVoiceState e)
        {
            User.Apply(e.User);
            Nickname = e.Nick;
            Volume = e.Volume;
            IsServerMuted = e.VoiceState.Mute;
            IsSelfMuted = e.VoiceState.SelfMute;
            IsServerDeafened = e.VoiceState.Deaf;
            IsSelfDeafened = e.VoiceState.SelfDeaf;
            IsSupressed = e.VoiceState.Suppress;
            IsMutedByMe = e.Mute;
        }
    }
}
