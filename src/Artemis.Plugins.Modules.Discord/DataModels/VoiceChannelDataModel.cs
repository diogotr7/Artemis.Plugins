using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordVoiceChannelDataModel : DataModel
{
    public string Id { get; set; }
    public string GuildId { get; set; }
    public string Name { get; set; }
    public int Type { get; set; }
    public int Bitrate { get; set; }
    public int UserLimit { get; set; }
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
}