using Artemis.Core.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Artemis.Core;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public sealed class ChromaService : IPluginService, IDisposable
{
    private readonly SKColor[][] _matrices;
    private readonly List<string> _apps;
    private readonly List<uint> _pids;
    private readonly ChromaReader _reader;
    private readonly Profiler _profiler;
    private readonly object _lock;
    
    public event EventHandler<RzDeviceType>? MatrixUpdated;
    public event EventHandler? AppListUpdated;
    
    public bool IsActive => !string.IsNullOrWhiteSpace(CurrentApp) && CurrentApp != "Artemis.UI.Windows.exe";
    public string? CurrentApp { get; private set; }
    public uint? CurrentAppId { get; private set; }
    public IEnumerable<string> AppNames => _apps;
    public IEnumerable<uint> AppIds => _pids; 
    
    public SKColor[][] Matrices => _matrices;
    
    public ChromaService(Plugin plugin)
    {
        _profiler = plugin.GetProfiler("Chroma Service");
        _lock = new object();
        _apps = new();
        _pids = new();
        
        var deviceTypes = Enum.GetValues<RzDeviceType>();
        _matrices = new SKColor[deviceTypes.Length][];
        for (var i = 0; i < _matrices.Length; i++)
            _matrices[i] = new SKColor[deviceTypes[i].GetLength()];

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
            var matrix = _matrices[(int) deviceType];
            
            Span<ChromaColor> colors = stackalloc ChromaColor[matrix.Length];
            
            data.GetColors(colors);

            for (var i = 0; i < colors.Length; i++)
            {
                var rzColor = colors[i];
                matrix[i] = new SKColor(rzColor.R, rzColor.G, rzColor.B);
            }

            MatrixUpdated?.Invoke(this, deviceType);
        }

        _profiler.StopMeasurement(profilerName);
    }

    private void UpdateAppListData(in ChromaAppData app)
    {
        _apps.Clear();
        _pids.Clear();

        CurrentAppId = app.CurrentAppId;
        CurrentApp = app.CurrentAppName;
        
        foreach(ref readonly var appInfo in app.AppInfo)
        {
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