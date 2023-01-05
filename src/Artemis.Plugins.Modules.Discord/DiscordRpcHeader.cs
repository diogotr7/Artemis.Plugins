using Artemis.Plugins.Modules.Discord.Enums;

namespace Artemis.Plugins.Modules.Discord;

#pragma warning disable CS0649

internal readonly struct DiscordRpcHeader
{
    public readonly RpcPacketType PacketType;
    public readonly int PacketLength;
}
