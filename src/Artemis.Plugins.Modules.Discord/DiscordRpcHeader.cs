using Artemis.Plugins.Modules.Discord.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord
{
    internal readonly struct DiscordRpcHeader
    {
        public readonly RpcPacketType PacketType;
        public readonly int PacketLength;
    }
}
