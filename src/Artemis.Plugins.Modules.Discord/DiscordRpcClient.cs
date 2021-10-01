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
        private static readonly string[] SCOPES = new string[]
        {
            "rpc",
            "identify",
            "rpc.notifications.read"
        };
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
        private readonly Task _readLoopTask;
        private readonly string _clientId;

        public event EventHandler<DiscordEvent> EventReceived;
        public event EventHandler<Exception> Error;

        public DiscordRpcClient(string clientId)
        {
            _clientId = clientId;
            _pendingRequests = new();
            _cancellationTokenSource = new();
            _pipe = new(".", PIPE, PipeDirection.InOut, PipeOptions.None);

            _pipe.Connect(500);
            SendInitPacket().Wait();
            _readLoopTask = Task.Run(ReadLoop);
            //send init packet, anything after this
            //should be handled with events from EventReceived
        }

        public async Task<DiscordResponse<T>> SendRequest<T>(DiscordRequest request) where T : class
        {
            var response = await SendRequestAsync(request);

            return (DiscordResponse<T>)response;
        }

        private async Task SendInitPacket()
        {
            var initPacket = JsonConvert.SerializeObject(new { v = RPC_VERSION, client_id = _clientId }, _jsonSerializerSettings);

            await SendPacketAsync(initPacket, RpcPacketType.HANDSHAKE);
        }

        private async Task ReadLoop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && _pipe.IsConnected)
            {
                byte[] headerBuffer = null;
                byte[] dataBuffer = null;
                try
                {
                    headerBuffer = ArrayPool<byte>.Shared.Rent(HEADER_SIZE);

                    await _pipe.ReadAsyncCancellable(headerBuffer, 0, HEADER_SIZE, _cancellationTokenSource.Token);

                    var opCode = (RpcPacketType)BitConverter.ToInt32(headerBuffer.AsSpan(0, 4));
                    var dataLength = BitConverter.ToInt32(headerBuffer.AsSpan(4, 4));

                    if (dataLength == 0)//if this is zero it means the pipe closed
                        break;

                    dataBuffer = ArrayPool<byte>.Shared.Rent(dataLength);

                    await _pipe.ReadAsyncCancellable(dataBuffer, 0, dataLength, _cancellationTokenSource.Token);

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
            if (opCode == RpcPacketType.HANDSHAKE)
            {
                if (string.IsNullOrEmpty(data))
                {
                    //probably close?
                }
                else
                {
                    //probably restart?
                }
                //SendPacket(new { v = RPC_VERSION, client_id = clientId.Value }, RpcPacketType.HANDSHAKE);
                //happens when closing discord and artemis is open?
                //TODO: investigate
            }
            if (opCode == RpcPacketType.CLOSE)
            {
                Error?.Invoke(this, new Exception($"Discord sent RpcPacketType.CLOSE: {data}"));
                return;
            }

            if (data.Contains("ERROR"))
                throw new Exception();

            IDiscordMessage discordMessage;
            try
            {
                discordMessage = JsonConvert.DeserializeObject<IDiscordMessage>(data, _jsonSerializerSettings);
            }
            catch (Exception exc)
            {
                Error?.Invoke(this, new Exception($"Error deserializing discord message: {data}", exc));
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
                Error?.Invoke(this, new Exception($"Discord message was neither response nor event: {data}"));
            }
        }

        private async Task SendPacketAsync(string stringData, RpcPacketType rpcPacketType)
        {
            var stringByteLength = Encoding.UTF8.GetByteCount(stringData);
            var bufferSize = HEADER_SIZE + stringByteLength;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            if (!BitConverter.TryWriteBytes(new Span<byte>(buffer, 0, 4), (int)rpcPacketType))
                throw new Exception("Error Writing rpc packet type.");
            if (!BitConverter.TryWriteBytes(new Span<byte>(buffer, 4, 4), (int)stringByteLength))
                throw new Exception("Error writing string byte length.");
            if (Encoding.UTF8.GetBytes(stringData, 0, stringData.Length, buffer, HEADER_SIZE) != stringData.Length)
                throw new Exception("Wrote wrong number of characters.");

            try
            {
                await _pipe.WriteAsync(buffer, 0, bufferSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public async Task<DiscordResponse> SendRequestAsync(DiscordRequest request)
        {
            var responseCompletionSource = new TaskCompletionSource<DiscordResponse>();

            //add this guid to the pending requests
            _pendingRequests.Add(request.Nonce, responseCompletionSource);

            //and send the actual request to the discord client.
            await SendPacketAsync(JsonConvert.SerializeObject(request, _jsonSerializerSettings), RpcPacketType.FRAME);

            var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            timeoutToken.Token.Register(() => responseCompletionSource.TrySetException(new TimeoutException()));

            //this will wait until the response with the expected Guid is received
            //and processed by the read loop.
            return await responseCompletionSource.Task;
        }

        private void HandlePendingRequest(DiscordResponse message)
        {
            if (_pendingRequests.TryGetValue(message.Nonce, out var tcs))
            {
                tcs.SetResult(message);
                _pendingRequests.Remove(message.Nonce);
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

                        //windows is stupid: https://stackoverflow.com/questions/52632448/namedpipeserverstream-readasync-does-not-exit-when-cancellationtoken-requests
                        try { _readLoopTask.Wait(); }
                        catch { }//catch everything

                        _cancellationTokenSource.Dispose();
                        _pipe.Dispose();
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
