namespace Artemis.Plugins.Modules.Discord.Enums;

public enum RpcPacketType : int
{
    HANDSHAKE = 0,
    FRAME = 1,
    CLOSE = 2,
    PING = 3,
    PONG = 4,
}
