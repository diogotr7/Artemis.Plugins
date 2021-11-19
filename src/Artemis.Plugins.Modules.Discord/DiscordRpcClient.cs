using Artemis.Plugins.Modules.Discord.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord
{
    public class DiscordRpcClient : IDisposable
    {
        #region RPC Constants
        private const string PIPE = "discord-ipc-0";
        private const string RPC_VERSION = "1";
        private const int HEADER_SIZE = 8;
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
            }
        };
        #endregion

        private readonly Dictionary<Guid, TaskCompletionSource<DiscordResponse>> _pendingRequests;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly NamedPipeClientStream _pipe;
        private readonly string _clientId;
        private Task _readLoopTask;

        public event EventHandler<DiscordEvent> EventReceived;
        public event EventHandler<Exception> Error;

        public DiscordRpcClient(string clientId)
        {
            _clientId = clientId;
            _pendingRequests = new();
            _cancellationTokenSource = new();
            _pipe = new(".", PIPE, PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public void Connect(int timeoutMs = 500)
        {
            _pipe.Connect(timeoutMs);
            _readLoopTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);
        }

        public async Task<DiscordResponse<T>> SendRequestAsync<T>(DiscordRequest request, int timeoutMs = 1000) where T : class
        {
            DiscordResponse response = await SendRequestAsync(request, timeoutMs);

            if (response is not DiscordResponse<T> typedResponse)
                throw new DiscordRpcClientException("Discord response was not of the specified type.", new InvalidCastException());

            return typedResponse;
        }

        private async Task SendInitPacket()
        {
            string initPacket = JsonConvert.SerializeObject(new { v = RPC_VERSION, client_id = _clientId }, _jsonSerializerSettings);

            await SendPacketAsync(initPacket, RpcPacketType.HANDSHAKE);
        }

        private async Task ReadLoop()
        {
            await SendInitPacket();

            while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
            {
                byte[] headerBuffer = null;
                byte[] dataBuffer = null;
                try
                {
                    headerBuffer = ArrayPool<byte>.Shared.Rent(HEADER_SIZE);

                    int headerReadBytes = await _pipe.ReadAsync(headerBuffer.AsMemory(0, HEADER_SIZE), _cancellationTokenSource.Token);

                    if (headerReadBytes < 4)
                        throw new DiscordRpcClientException("Read less than 4 bytes for the header");

                    RpcPacketType opCode = (RpcPacketType)BitConverter.ToInt32(headerBuffer.AsSpan(0, 4));
                    int dataLength = BitConverter.ToInt32(headerBuffer.AsSpan(4, 4));

                    if (dataLength == 0)
                        throw new DiscordRpcClientException("Read zero bytes from the pipe");

                    dataBuffer = ArrayPool<byte>.Shared.Rent(dataLength);

                    await _pipe.ReadAsync(dataBuffer.AsMemory(0, dataLength), _cancellationTokenSource.Token);

                    await ProcessPipeMessageAsync(opCode, Encoding.UTF8.GetString(dataBuffer.AsSpan(0, dataLength)));
                }
                catch (Exception e)
                {
                    Error?.Invoke(this, e);
                }
                finally
                {
                    if (headerBuffer != null)
                        ArrayPool<byte>.Shared.Return(headerBuffer);
                    if (dataBuffer != null)
                        ArrayPool<byte>.Shared.Return(dataBuffer);
                }
            }
        }

        private async Task ProcessPipeMessageAsync(RpcPacketType opCode, string data)
        {
            if (opCode == RpcPacketType.PING)
            {
                await SendPacketAsync(data, RpcPacketType.PONG);
                return;
            }
            if (opCode == RpcPacketType.CLOSE)
            {
                Error?.Invoke(this, new DiscordRpcClientException($"Discord sent RpcPacketType.CLOSE: {data}"));
                return;
            }
            //if (opCode == RpcPacketType.HANDSHAKE)
            //{
            //if (string.IsNullOrEmpty(data))
            //{
            //probably close?
            //}
            //else
            //{
            //probably restart?
            //}
            //SendPacket(new { v = RPC_VERSION, client_id = clientId.Value }, RpcPacketType.HANDSHAKE);
            //happens when closing discord and artemis is open?
            //TODO: investigate
            //}

            if (data.Contains("\"evt\":\"ERROR\""))//this looks kinda stupid ¯\_(ツ)_/¯
                throw new DiscordRpcClientException($"Discord response contained an error: {data}");

            IDiscordMessage discordMessage;
            try
            {
                discordMessage = JsonConvert.DeserializeObject<IDiscordMessage>(data, _jsonSerializerSettings);
            }
            catch (Exception exc)
            {
                Error?.Invoke(this, new DiscordRpcClientException($"Error deserializing discord message: {data}", exc));
                return;
            }

            if (discordMessage is DiscordResponse discordResponse)
            {
                HandlePendingRequest(discordResponse);
            }
            else if (discordMessage is DiscordEvent discordEvent)
            {
                EventReceived?.Invoke(this, discordEvent);
            }
            else
            {
                Error?.Invoke(this, new DiscordRpcClientException($"Discord message was neither response nor event: {data}"));
            }
        }

        private async Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType)
        {
            int stringByteLength = Encoding.UTF8.GetByteCount(stringData);
            int bufferSize = HEADER_SIZE + stringByteLength;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            if (!BitConverter.TryWriteBytes(new Span<byte>(buffer, 0, 4), (int)rpcPacketType))
                throw new DiscordRpcClientException("Error writing rpc packet type.");

            if (!BitConverter.TryWriteBytes(new Span<byte>(buffer, 4, 4), (int)stringByteLength))
                throw new DiscordRpcClientException("Error writing string byte length.");

            if (Encoding.UTF8.GetBytes(stringData, 0, stringData.Length, buffer, HEADER_SIZE) != stringData.Length)
                throw new DiscordRpcClientException("Wrote wrong number of characters.");

            try
            {
                await _pipe.WriteAsync(buffer, 0, bufferSize, _cancellationTokenSource.Token);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private async Task<DiscordResponse> SendRequestAsync(DiscordRequest request, int timeoutMs)
        {
            TaskCompletionSource<DiscordResponse> responseCompletionSource = new TaskCompletionSource<DiscordResponse>();

            //add this guid to the pending requests
            _pendingRequests.Add(request.Nonce, responseCompletionSource);

            //and send the actual request to the discord client.
            await SendPacketAsync(JsonConvert.SerializeObject(request, _jsonSerializerSettings), RpcPacketType.FRAME);

            CancellationTokenSource timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
            timeoutToken.Token.Register(() => responseCompletionSource.TrySetException(new TimeoutException($"Discord request timed out after {timeoutMs}")));

            //this will wait until the response with the expected Guid is received
            //and processed by the read loop.
            return await responseCompletionSource.Task;
        }

        private void HandlePendingRequest(DiscordResponse message)
        {
            if (_pendingRequests.TryGetValue(message.Nonce, out TaskCompletionSource<DiscordResponse> tcs))
            {
                if (!tcs.TrySetResult(message))
                    Error?.Invoke(this, new DiscordRpcClientException("Failed to set task result. Perhaps the task timed out?"));

                _pendingRequests.Remove(message.Nonce);
            }
            else
            {
                Error?.Invoke(this, new DiscordRpcClientException("Received response with unknown guid"));
            }
        }

        #region IDisposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        _cancellationTokenSource.Cancel();
                        _pipe.Dispose();

                        try { _readLoopTask.Wait(); }
                        catch { }//catch everything

                        _cancellationTokenSource.Dispose();
                    }
                    catch
                    {
                        //¯\_(ツ)_/¯
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
