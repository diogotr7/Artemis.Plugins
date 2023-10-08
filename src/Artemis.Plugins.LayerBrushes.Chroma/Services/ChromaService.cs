using Artemis.Core.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Artemis.Core;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public sealed class ChromaService : IPluginService, IDisposable
{
    private readonly Dictionary<RzDeviceType, SKColor[,]> _matrices;
    private readonly List<string> _apps;
    private readonly List<uint> _pids;
    private readonly ChromaReader _reader;
    private readonly Profiler _profiler;
    private readonly object _lock;
    
    public event EventHandler<MatrixUpdatedEventArgs>? MatrixUpdated;
    public event EventHandler? AppListUpdated;
    
    public bool IsActive => !string.IsNullOrWhiteSpace(CurrentApp) && CurrentApp != "Artemis.UI.Windows.exe";
    public string? CurrentApp { get; private set; }
    public uint? CurrentAppId { get; private set; }
    public IEnumerable<string> AppNames => _apps;
    public IEnumerable<uint> AppIds => _pids; 
    
    public ChromaService(Plugin plugin)
    {
        _profiler = plugin.GetProfiler("Chroma Service");
        _lock = new object();
        _matrices = new();
        _apps = new();
        _pids = new();

        _reader = new();
        _reader.KeyboardUpdated += RazerEmulatorReaderOnKeyboardUpdated;
        _reader.MouseUpdated += RazerEmulatorReaderOnMouseUpdated;
        _reader.MousepadUpdated += RazerEmulatorReaderOnMousepadUpdated;
        _reader.KeypadUpdated += RazerEmulatorReaderOnKeypadUpdated;
        _reader.HeadsetUpdated += RazerEmulatorReaderOnHeadsetUpdated;
        _reader.ChromaLinkUpdated += RazerEmulatorReaderOnChromaLinkUpdated;
        _reader.AppDataUpdated += RazerEmulatorReaderOnAppDataUpdated;
        _reader.Start();
    }

    private void RazerEmulatorReaderOnAppDataUpdated(object? sender, in ChromaAppData e) => UpdateAppListData(in e);

    private void RazerEmulatorReaderOnChromaLinkUpdated(object? sender, in ChromaLink e) => UpdateMatrix(RzDeviceType.ChromaLink, in e);

    private void RazerEmulatorReaderOnHeadsetUpdated(object? sender, in ChromaHeadset e) => UpdateMatrix(RzDeviceType.Headset, in e);

    private void RazerEmulatorReaderOnKeypadUpdated(object? sender, in ChromaKeypad e) => UpdateMatrix(RzDeviceType.Keypad, in e);

    private void RazerEmulatorReaderOnMousepadUpdated(object? sender, in ChromaMousepad e) => UpdateMatrix(RzDeviceType.Mousepad, in e);

    private void RazerEmulatorReaderOnMouseUpdated(object? sender, in ChromaMouse e) => UpdateMatrix(RzDeviceType.Mouse, in e);

    private void RazerEmulatorReaderOnKeyboardUpdated(object? sender, in ChromaKeyboard e) => UpdateMatrix(RzDeviceType.Keyboard, in e);

    private void UpdateMatrix<T>(RzDeviceType deviceType, in T data) where T :  IColorProvider
    {
        var profilerName = deviceType.ToStringFast();
        
        _profiler.StartMeasurement(profilerName);
        lock (_lock)
        {
            if (!_matrices.TryGetValue(deviceType, out var matrix))
            {
                matrix = new SKColor[data.Height, data.Width];
                _matrices.Add(deviceType, matrix);
            }
            
            for (var i = 0; i < data.Height; i++)
            {
                for (var j = 0; j < data.Width; j++)
                {
                    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                    var clr = data.GetColor(i * data.Width + j);
                    matrix[i, j] = new SKColor(clr.R, clr.G, clr.B);
                }
            }
            MatrixUpdated?.Invoke(this, new MatrixUpdatedEventArgs
            {
                Matrix = matrix,
                DeviceType = deviceType
            });
        }

        _profiler.StopMeasurement(profilerName);
    }

    private void UpdateAppListData(in ChromaAppData app)
    {
        _apps.Clear();
        _pids.Clear();

        CurrentAppId = app.CurrentAppId;
        CurrentApp = app.CurrentAppName;

        var span = app.AppInfo.AsSpan();
        for (var i = 0; i < app.AppCount; i++)
        {
            ref var appInfo = ref span[i];
            
            _apps.Add(appInfo.AppName);
            _pids.Add(appInfo.AppId);
        }
        
        AppListUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _reader.KeyboardUpdated -= RazerEmulatorReaderOnKeyboardUpdated;
        _reader.MouseUpdated -= RazerEmulatorReaderOnMouseUpdated;
        _reader.MousepadUpdated -= RazerEmulatorReaderOnMousepadUpdated;
        _reader.KeypadUpdated -= RazerEmulatorReaderOnKeypadUpdated;
        _reader.HeadsetUpdated -= RazerEmulatorReaderOnHeadsetUpdated;
        _reader.ChromaLinkUpdated -= RazerEmulatorReaderOnChromaLinkUpdated;
        _reader.AppDataUpdated -= RazerEmulatorReaderOnAppDataUpdated;
        _reader.Dispose();
    }
}