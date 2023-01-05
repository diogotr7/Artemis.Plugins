namespace Artemis.Plugins.Modules.Discord.Enums;

internal enum RpcPacketType : int
{
    HANDSHAKE = 0,
    FRAME = 1,
    CLOSE = 2,
    PING = 3,
    PONG = 4,
}
