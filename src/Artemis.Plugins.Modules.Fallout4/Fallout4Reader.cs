using Artemis.Plugins.Modules.Fallout4.Enums;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Artemis.Plugins.Modules.Fallout4
{
    internal class Fallout4Reader : IDisposable
    {
        private readonly Task _readLoop;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly SortedDictionary<uint, FalloutValue> _database;
        private readonly Timer _heartbeatTimer = new(5000);
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly byte[] _heartbeatPacket = new byte[5];
        private readonly byte[] _headerBuffer = new byte[5];

        public ReadOnlyDictionary<uint, FalloutValue> Database { get; }
        public event EventHandler<FalloutInitPacket> Connected;
        public event EventHandler DataReceived;
        public event EventHandler<uint> FieldRemoved;
        public event EventHandler<Exception> Exception;

        public Fallout4Reader()
        {
            _database = new();
            Database = new(_database);

            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;

            _tcpClient = new TcpClient("127.0.0.1", 27000);
            _stream = _tcpClient.GetStream();

            _cancellationTokenSource = new();
            _readLoop = Task.Run(ReadLoop);
        }

        private async void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _stream.WriteAsync(_heartbeatPacket);
            }
            catch { }
        }

        public async Task ReadLoop()
        {
            while (!_cancellationTokenSource.IsCancellationRequested && _tcpClient.Connected)
            {
                await _stream.ReadAsync(_headerBuffer);

                var messageSize = (int)BitConverter.ToUInt32(_headerBuffer, 0);
                var commandType = (FalloutPacketType)_headerBuffer[4];

                if (commandType == FalloutPacketType.Heartbeat)
                {
                    //if we get a heartbeat command, send the same bytes back
                    await _stream.WriteAsync(_headerBuffer);
                    continue;
                }

                if (messageSize == 0)
                {
                    throw new Exception("Unexpected zero length message");
                }

                var buffer = ArrayPool<byte>.Shared.Rent(messageSize);
                try
                {
                    await _stream.ReadAsync(buffer, 0, messageSize);

                    switch (commandType)
                    {
                        case FalloutPacketType.NewConnection:
                            var initText = JsonConvert.DeserializeObject<FalloutInitPacket>(Encoding.UTF8.GetString(buffer, 0, messageSize));
                            Connected?.Invoke(this, initText);
                            break;
                        case FalloutPacketType.DataUpdate:
                            UpdateDatabase(buffer.AsSpan(0, messageSize));
                            DataReceived?.Invoke(this, EventArgs.Empty);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Exception?.Invoke(this, e);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        private void UpdateDatabase(Span<byte> span)
        {
            var reader = new SpanReader(span);

            while (reader.Offset < reader.Data.Length)
            {
                var dataType = (FalloutDataType)reader.ReadByte();
                var dataId = reader.ReadUInt32();

                if (dataType == FalloutDataType.Map)
                {
                    //Map is a special snowflake.
                    //Keys can be added and removed in the same Update.
                    //in this case, we have to remove first before adding
                    //so the value is replaced.
                    var map = reader.ReadMap();

                    var removeList = reader.ReadArray();
                    foreach (var removeKey in removeList)
                    {
                        _database.Remove(removeKey);
                        FieldRemoved?.Invoke(this, removeKey);
                    }

                    _database[dataId] = new MapFalloutElement(map);
                }
                else
                {
                    _database[dataId] = dataType switch
                    {
                        FalloutDataType.Boolean => new BoolFalloutElement(reader.ReadBoolean()),
                        FalloutDataType.SByte => new SByteFalloutElement(reader.ReadSByte()),
                        FalloutDataType.Byte => new ByteFalloutElement(reader.ReadByte()),
                        FalloutDataType.Int => new IntFalloutElement(reader.ReadInt32()),
                        FalloutDataType.UInt => new UIntFalloutElement(reader.ReadUInt32()),
                        FalloutDataType.Float => new FloatFalloutElement(reader.ReadSingle()),
                        FalloutDataType.String => new StringFalloutElement(reader.ReadNullTerminatedString()),
                        FalloutDataType.Array => new ArrayFalloutElement(reader.ReadArray()),
                        _ => throw new ArgumentException()
                    };
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                _readLoop.Wait();
            }
            catch { }
            _cancellationTokenSource?.Dispose();
            _heartbeatTimer?.Dispose();
            _stream?.Dispose();
            _tcpClient?.Dispose();
            _database.Clear();
        }
    }

    public record FalloutInitPacket(string lang, string version);
}
