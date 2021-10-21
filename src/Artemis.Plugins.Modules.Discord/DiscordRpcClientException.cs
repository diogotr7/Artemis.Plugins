using System;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordRpcClientException : Exception
    {
        public DiscordRpcClientException() : base()
        {
        }

        public DiscordRpcClientException(string message) : base(message)
        {
        }

        public DiscordRpcClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
