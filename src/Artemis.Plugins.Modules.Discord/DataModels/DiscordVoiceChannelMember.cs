using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordVoiceChannelMembers : DataModel
{

    
    //DynamicDataModel with the other members
}

public class DiscordVoiceChannelMember
{
    public DiscordUserDataModel User { get; } = new();
    public string Nickname { get; set; } = string.Empty;
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

    public void Reset()
    {
        IsServerMuted = false;
        IsSelfMuted = false;
        IsServerDeafened = false;
        IsSelfDeafened = false;
        IsSpeaking = false;
        IsSupressed = false;
        IsMutedByMe = false;
    }
}
