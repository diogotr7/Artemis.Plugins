using Artemis.Plugins.Modules.Discord.Enums;

namespace Artemis.Plugins.Modules.Discord;

internal readonly struct DiscordRpcHeader
{
    public readonly RpcPacketType PacketType;
    public readonly int PacketLength;
}
