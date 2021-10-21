using Artemis.Plugins.Modules.Discord.Enums;
using JsonSubTypes;
using Newtonsoft.Json;
using System;
using static JsonSubTypes.JsonSubtypes;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordResponse : IDiscordMessage
    {
        [JsonProperty("cmd")]
        public DiscordRpcCommand Command { get; set; }

        public Guid Nonce { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "cmd")]
    [KnownSubType(typeof(DiscordResponse<Authorize>), DiscordRpcCommand.AUTHORIZE)]
    [KnownSubType(typeof(DiscordResponse<Authenticate>), DiscordRpcCommand.AUTHENTICATE)]
    [KnownSubType(typeof(DiscordResponse<VoiceSettings>), DiscordRpcCommand.GET_VOICE_SETTINGS)]
    [KnownSubType(typeof(DiscordResponse<Subscribe>), DiscordRpcCommand.SUBSCRIBE)]
    [KnownSubType(typeof(DiscordResponse<SelectedVoiceChannel>), DiscordRpcCommand.GET_SELECTED_VOICE_CHANNEL)]
    public class DiscordResponse<T> : DiscordResponse
    {
        public T Data { get; init; }
    }
}
