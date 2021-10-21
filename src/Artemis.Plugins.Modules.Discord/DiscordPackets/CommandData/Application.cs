﻿using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record Application
    (
        string Id,
        string Name,
        string Icon,
        string Description,
        string Summary,
        bool Hook,
        List<string> RpcOrigins,
        string VerifyKey
    );
}
