using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Plugins.Modules.Discord.Enums;

namespace Artemis.Plugins.Modules.Discord.Transport;

public interface IDiscordTransport : IDisposable
{
    bool IsConnected { get; }
    Task Connect(CancellationToken cancellationToken = default);
    Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType, CancellationToken cancellationToken = default);
    Task<(RpcPacketType, string)> ReadMessageAsync(CancellationToken cancellationToken = default);
}