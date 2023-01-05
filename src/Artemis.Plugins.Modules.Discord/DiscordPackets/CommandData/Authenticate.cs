using System;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record Authenticate
(
    User User,
    Application Application,
    DateTime Expires,
    string AccessToken
);
