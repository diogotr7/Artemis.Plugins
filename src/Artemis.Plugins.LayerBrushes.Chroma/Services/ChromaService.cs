using Artemis.Core.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Artemis.Core;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public record MatrixUpdatedEventArgs(RzDeviceType DeviceType, SKColor[] Matrix);

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class ChromaService : IPluginService, IDisposable
{
    private readonly SKColor[][] _matrices;
    private readonly ChromaReader _reader;
    private readonly Profiler _profiler;
    private readonly object _lock;
    private readonly MatrixUpdatedEventArgs[] _matrixUpdatedEventArgs;

    public event EventHandler<MatrixUpdatedEventArgs>? MatrixUpdated;
    public event EventHandler? AppListUpdated;

    public bool IsActive => !string.IsNullOrWhiteSpace(CurrentApp) && CurrentApp != "Artemis.UI.Windows.exe";
    public string? CurrentApp { get; private set; }
    public uint? CurrentAppId { get; private set; }
    public string[] AppNames { get; }
    public uint[] AppIds { get; }

    public ChromaService(Plugin plugin)
    {
        _profiler = plugin.GetProfiler("Chroma Service");
        _lock = new object();
        AppNames = new string[50];
        AppIds = new uint[50];

        var deviceTypes = Enum.GetValues<RzDeviceType>();
        _matrices = deviceTypes.Select(deviceType => new SKColor[deviceType.GetLength()]).ToArray();
        _matrixUpdatedEventArgs = deviceTypes.Select(deviceType => new MatrixUpdatedEventArgs(deviceType, _matrices[(int)deviceType])).ToArray();

        _reader = new ChromaReader();
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

    private void UpdateMatrix<T>(RzDeviceType deviceType, in T data) where T : IColorProvider
    {
        var profilerName = deviceType.ToStringFast();

        _profiler.StartMeasurement(profilerName);
        
        lock (_lock)
        {
            var matrix = _matrices[(int)deviceType];
            //SKColor and ChromaColor are the same size and layout,
            //so we can just write to it directly.
            var colors = MemoryMarshal.Cast<SKColor, ChromaColor>(matrix);
            data.GetColors(colors);

            MatrixUpdated?.Invoke(this, _matrixUpdatedEventArgs[(int)deviceType]);
        }

        _profiler.StopMeasurement(profilerName);
    }

    private void UpdateAppListData(in ChromaAppData app)
    {
        _profiler.StartMeasurement("AppList");
        
        CurrentAppId = app.CurrentAppId;
        CurrentApp = app.CurrentAppName;

        ReadOnlySpan<ChromaAppInfo> apps = app.AppInfo;
        for (var i = 0; i < apps.Length; i++)
        {
            ref readonly var appInfo = ref apps[i];
            AppNames[i] = appInfo.AppName;
            AppIds[i] = appInfo.AppId;
        }

        AppListUpdated?.Invoke(this, EventArgs.Empty);
        
        _profiler.StopMeasurement("AppList");
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