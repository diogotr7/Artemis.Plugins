using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Transport;

public sealed class DiscordPipeTransport : IDiscordTransport
{
    private const string RpcVersion = "1";
    private const int HeaderSize = 8;
    
    private readonly byte[] _headerBuffer = new byte[8];
    private readonly string _clientId;
    private NamedPipeClientStream? _pipe;
    public bool IsConnected => _pipe?.IsConnected == true;
    
    public DiscordPipeTransport(string clientId)
    {
        _clientId = clientId;
    }

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        const int MAX_TRIES = 10;
        for (var i = 0; i < MAX_TRIES; i++)
        {
            try
            {
                _pipe = new NamedPipeClientStream(".", GetPipeName(i), PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipe.ConnectAsync(cancellationToken);
                break;
            }
            catch (Exception e)
            {
                //TODO: how to log here?? ignore?
                Debug.WriteLine($"Error connecting to pipe {i}: {e.Message}");
            }
        }
        
        var handshake = JsonConvert.SerializeObject(new { v = RpcVersion, client_id = _clientId });

        await SendPacketAsync(handshake, RpcPacketType.HANDSHAKE, cancellationToken);
    }

    public async Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType, CancellationToken cancellationToken = default)
    {
        int stringByteLength = Encoding.UTF8.GetByteCount(stringData);
        int bufferSize = HeaderSize + stringByteLength;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            if (!BitConverter.TryWriteBytes(buffer.AsSpan(0, 4), (int)rpcPacketType))
                throw new DiscordRpcClientException("Error writing rpc packet type.");

            if (!BitConverter.TryWriteBytes(buffer.AsSpan(4, 4), stringByteLength))
                throw new DiscordRpcClientException("Error writing string byte length.");

            if (Encoding.UTF8.GetBytes(stringData, 0, stringData.Length, buffer, HeaderSize) != stringData.Length)
                throw new DiscordRpcClientException("Wrote wrong number of characters.");

            await _pipe.WriteAsync(buffer.AsMemory(0, bufferSize), cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public async Task<(RpcPacketType, string)> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        byte[]? dataBuffer = null;
        try
        {
            int headerReadBytes = await _pipe.ReadAsync(_headerBuffer.AsMemory(0, HeaderSize));

            if (headerReadBytes < HeaderSize)
                throw new DiscordRpcClientException("Read less than 4 bytes for the header");

            var header = MemoryMarshal.AsRef<DiscordRpcHeader>(_headerBuffer);

            if (header.PacketLength == 0)
                throw new DiscordRpcClientException("Read zero bytes from the pipe");

            dataBuffer = ArrayPool<byte>.Shared.Rent(header.PacketLength);

            await _pipe.ReadAsync(dataBuffer.AsMemory(0, header.PacketLength), cancellationToken);

            return (header.PacketType, Encoding.UTF8.GetString(dataBuffer.AsSpan(0, header.PacketLength)));
        }
        finally
        {
            if (dataBuffer != null)
                ArrayPool<byte>.Shared.Return(dataBuffer);
        }
    }

    public void Dispose()
    {
        _pipe.Dispose();
    }
    
    
    private static string GetPipeName(int index)
    {
        const string PIPE_NAME = "discord-ipc-{0}";
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Unix => Path.Combine(GetTemporaryDirectory(), string.Format(PIPE_NAME, index)),
            _ => string.Format(PIPE_NAME, index)
        };
    }

    private static string GetTemporaryDirectory()
    {
        //source: https://github.com/Lachee/discord-rpc-csharp/
        //try all these possible paths it could be, depending on system configuration
        return Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ??
               Environment.GetEnvironmentVariable("TMPDIR") ??
               Environment.GetEnvironmentVariable("TMP") ??
               Environment.GetEnvironmentVariable("TEMP") ??
               "/tmp";
    }

}