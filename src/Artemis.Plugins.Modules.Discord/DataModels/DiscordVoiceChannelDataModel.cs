﻿using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordVoiceChannelDataModel : DataModel
{
    public string Id { get; set; } = string.Empty;
    public string GuildId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Bitrate { get; set; }
    public int UserLimit { get; set; }
    public bool IsAnyoneElseSpeaking { get; internal set; }
    public DiscordVoiceChannelMember Me { get; internal set; } = new();
    [DataModelProperty(Name = "Other Members")]
    public DiscordVoiceChannelMembers Members { get; } = new();

    internal void Apply(SelectedVoiceChannel e)
    {
        Id = e.Id;
        GuildId = e.GuildId;
        Name = e.Name;
        Type = e.Type;
        Bitrate = e.Bitrate;
        UserLimit = e.UserLimit;
    }

    public void Reset()
    {
        Id = string.Empty;
        GuildId = string.Empty;
        Name = string.Empty;
        Type = 0;
        Bitrate = 0;
        UserLimit = 0;
        IsAnyoneElseSpeaking = false;
        Me.Reset();
        Members.ClearDynamicChildren();
    }
}