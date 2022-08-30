using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordRequest
    {
        public Guid Nonce { get; }

        [JsonProperty("args")]
        public JObject Arguments { get; }

        [JsonProperty("cmd"), JsonConverter(typeof(StringEnumConverter))]
        public DiscordRpcCommand Command { get; }

        public DiscordRequest(DiscordRpcCommand command)
        {
            Nonce = Guid.NewGuid();
            Arguments = new JObject();
            Command = command;
        }

        public DiscordRequest WithArgument(string key, object value)
        {
            Arguments.Add(key, JToken.FromObject(value));
            return this;
        }
    }

    public class DiscordSubscribe : DiscordRequest
    {
        [JsonProperty("evt"), JsonConverter(typeof(StringEnumConverter))]
        public DiscordRpcEvent Event { get; }

        public DiscordSubscribe(DiscordRpcEvent e) : base(DiscordRpcCommand.SUBSCRIBE)
        {
            Event = e;
        }
    }

    public class DiscordUnsubscribe : DiscordRequest
    {
        [JsonProperty("evt"), JsonConverter(typeof(StringEnumConverter))]
        public DiscordRpcEvent Event { get; }

        public DiscordUnsubscribe(DiscordRpcEvent e) : base(DiscordRpcCommand.UNSUBSCRIBE)
        {
            Event = e;
        }
    }
}
