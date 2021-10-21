﻿using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record SelectedVoiceChannel(
        string Id,
        string GuildId,
        string Name,
        int Type,
        int Bitrate,
        int UserLimit,
        int Position,
        List<UserVoiceState> VoiceStates
    );
}