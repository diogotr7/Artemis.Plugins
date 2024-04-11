using System;

namespace Artemis.Plugins.Modules.Discord;

public class DiscordRpcClientException : Exception
{
    public bool ShouldReconnect { get; }

    public DiscordRpcClientException(string message, bool shouldReconnect = false) : base(message)
    {
        ShouldReconnect = shouldReconnect;
    }

    public DiscordRpcClientException(string message, Exception innerException, bool shouldReconnect = false) : base(message, innerException)
    {
        ShouldReconnect = shouldReconnect;
    }
}

public class DiscordException : Exception
{
    public DiscordException(string message) : base(message)
    {
    }

    public DiscordException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
