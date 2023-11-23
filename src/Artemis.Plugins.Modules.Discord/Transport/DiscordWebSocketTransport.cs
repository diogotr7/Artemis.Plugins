using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
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
        var length = Encoding.UTF8.GetByteCount(stringData);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        Encoding.UTF8.GetBytes(stringData, 0, stringData.Length, rent, 0);

        try
        {
            await _webSocket.SendAsync(rent.AsMemory(0, length), WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    public async Task<(RpcPacketType, string)> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            var rent = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                result = await _webSocket.ReceiveAsync(rent, cancellationToken);
                memoryStream.Write(rent, 0, result.Count);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rent);
            }
        } while (!result.EndOfMessage);
        
        if (result.MessageType != WebSocketMessageType.Text)
            throw new DiscordRpcClientException("WebSocket closed");

        var packetData = Encoding.UTF8.GetString(memoryStream.ToArray());

        return (RpcPacketType.FRAME, packetData);
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}