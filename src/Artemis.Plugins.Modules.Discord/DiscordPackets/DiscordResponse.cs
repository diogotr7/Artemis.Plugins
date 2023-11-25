using System;
using Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

namespace Artemis.Plugins.Modules.Discord.DiscordPackets;

public class DiscordResponse : IDiscordMessage
{
    public Guid Nonce { get; set; }
}

public abstract class DiscordResponse<T> : DiscordResponse
{
#pragma warning disable CS8618
    public T Data { get; init; }
#pragma warning restore CS8618
}

public sealed class DiscordAuthorizeResponse : DiscordResponse<Authorize> { }
public sealed class DiscordAuthenticateResponse : DiscordResponse<Authenticate> { }
public sealed class DiscordGetVoiceSettingsResponse : DiscordResponse<VoiceSettings> { }
public sealed class DiscordSubscribeResponse : DiscordResponse<Subscription> { }
public sealed class DiscordGetSelectedVoiceChannelResponse : DiscordResponse<SelectedVoiceChannel> { }