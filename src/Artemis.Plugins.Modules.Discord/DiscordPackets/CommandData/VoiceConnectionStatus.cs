﻿using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record Ping
    (
        ulong Time,
        uint Value
    );
    
    public record VoiceConnectionStatus
    (
        string State,
        string Hostname,
        List<Ping> Pings,
        float AveragePing,
        float? LastPing
    );
}