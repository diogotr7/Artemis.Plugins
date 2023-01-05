using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

public class DiscordRequest
{
    public Guid Nonce { get; }

    [JsonProperty("args")]
    public JObject Arguments { get; }

    [JsonProperty("cmd"), JsonConverter(typeof(StringEnumConverter))]
    public DiscordRpcCommand Command { get; }

    public DiscordRequest(DiscordRpcCommand command, params (string Key, object Value)[] parameters)
    {
        Nonce = Guid.NewGuid();
        Command = command;
        Arguments = new JObject();

        foreach (var (Key, Value) in parameters)
        {
            Arguments.Add(Key, JToken.FromObject(Value));
        }
    }
}

public class DiscordSubscribe : DiscordRequest
{
    [JsonProperty("evt"), JsonConverter(typeof(StringEnumConverter))]
    public DiscordRpcEvent Event { get; }

    public DiscordSubscribe(DiscordRpcEvent e, params (string Key, object Value)[] parameters)
        : base(DiscordRpcCommand.SUBSCRIBE, parameters)
    {
        Event = e;
    }
}

public class DiscordUnsubscribe : DiscordRequest
{
    [JsonProperty("evt"), JsonConverter(typeof(StringEnumConverter))]
    public DiscordRpcEvent Event { get; }

    public DiscordUnsubscribe(DiscordRpcEvent e, params (string Key, object Value)[] parameters)
        : base(DiscordRpcCommand.UNSUBSCRIBE, parameters)
    {
        Event = e;
    }
}
