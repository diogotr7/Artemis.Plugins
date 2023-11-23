using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Plugins.Modules.Discord.Enums;

namespace Artemis.Plugins.Modules.Discord.Transport;

public sealed class DiscordWebSocketTransport : IDiscordTransport
{
    private const string WebsocketUri = "ws://localhost:6463";
    
    private readonly ClientWebSocket _webSocket;
    private readonly string _clientId;
    private readonly string _origin;
    
    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    public DiscordWebSocketTransport(string clientId, string origin)
    {
        _webSocket = new ClientWebSocket();
        _clientId = clientId;
        _origin = origin;
    }

    public  async Task Connect(CancellationToken cancellationToken = default)
    {
        _webSocket.Options.SetRequestHeader("Origin", _origin);
        await _webSocket.ConnectAsync(new Uri($"{WebsocketUri}?v=1&client_id={_clientId}"), cancellationToken);
    }

    public async Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"Sending {rpcPacketType}: {stringData}");

        var buffer = Encoding.UTF8.GetBytes(stringData);

        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task<(RpcPacketType, string)> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        var rent = ArrayPool<byte>.Shared.Rent(32768);
        var result = await _webSocket.ReceiveAsync(rent, cancellationToken);

        if (result.MessageType == WebSocketMessageType.Close)
            throw new DiscordRpcClientException("WebSocket closed");

        var packetData = Encoding.UTF8.GetString(rent.AsSpan(0, result.Count));
        ArrayPool<byte>.Shared.Return(rent);

        Debug.WriteLine($"Received {result.MessageType}: {packetData}");

        return (RpcPacketType.FRAME, packetData);
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}