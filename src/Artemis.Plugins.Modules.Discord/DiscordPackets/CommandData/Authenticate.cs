using System;

namespace Artemis.Plugins.Modules.Discord
{
    public record Authenticate
    (
        User User,
        Application Application,
        DateTime Expires,
        string AccessToken
    );
}
