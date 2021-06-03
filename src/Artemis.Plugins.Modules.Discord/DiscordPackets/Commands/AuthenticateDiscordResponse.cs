using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Discord
{
    public record DiscordUser
    (
        string Username,
        string Discriminator,
        string Id,
        string Avatar,
        bool? Bot
    );

    public record DiscordApplication
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

    public record AuthenticateData
    (
        DiscordUser User,
        DiscordApplication Application,
        DateTime Expires,
        string AccessToken
    );
}
