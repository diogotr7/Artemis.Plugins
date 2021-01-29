using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public record SelectedVoiceChannelData(
        string Id,
        string GuildId,
        string Name,
        int Type,
        int Bitrate,
        int UserLimit,
        int Position,
        List<UserVoiceState> VoiceStates
    ) : DiscordMessageData;

    public record Pan(
        double Left,
        double Right
    );

    public record UserVoiceState(
        string Nick,
        bool Mute,
        int Volume,
        Pan Pan,
        VoiceState VoiceState,
        DiscordUser User
    );

    public record VoiceState(
        bool Mute,
        bool Deaf,
        bool SelfMute,
        bool SelfDeaf,
        bool Suppress
    );
}