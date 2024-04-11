using System;
using System.Buffers;
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

    public async Task Connect(CancellationToken cancellationToken = default)
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
        //this is a large enough initial chunk to hold most messages
        var buffer = ArrayPool<byte>.Shared.Rent(2048);
        var read = 0;
        try
        {
            ValueWebSocketReceiveResult result;
            do
            {
                result = await _webSocket.ReceiveAsync(buffer.AsMemory(read), cancellationToken);
                read += result.Count;

                //if we need more space, double the size
                if (read == buffer.Length)
                {
                    var newBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length * 2);
                    buffer.CopyTo(newBuffer, 0);
                    ArrayPool<byte>.Shared.Return(buffer);

                    buffer = newBuffer;
                }
            } while (!result.EndOfMessage);
            
            var packetData = Encoding.UTF8.GetString(buffer, 0, read);

            return (RpcPacketType.FRAME, packetData);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }
}