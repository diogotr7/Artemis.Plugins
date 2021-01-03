using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.DataModelExpansions.Discord.Enums
{
    internal enum RpcPacketType : int
    {
        HANDSHAKE = 0,
        FRAME = 1,
        CLOSE = 2,
        PING = 3,
        PONG = 4,
    }
}
