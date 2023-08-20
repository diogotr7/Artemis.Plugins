using Artemis.Core.Services;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public class ChromaService : IPluginService, IDisposable
{
    private readonly ILogger _logger;
    private readonly ChromaReader _reader;
    private readonly Profiler _profiler;
    private readonly Dictionary<RzDeviceType, string> _enumNames;

    public event EventHandler<RzDeviceType>? MatrixUpdated;
    public event EventHandler? AppListUpdated;

    public int? CurrentAppId { get; private set; } = null;
    public string? CurrentApp { get; private set; } = null;
    public List<string> Apps { get; } = new();
    public List<int> Pids { get; } = new();
    public ConcurrentDictionary<RzDeviceType, SKColor[,]> Matrices { get; } = new();

    public ChromaService(ILogger logger, Plugin plugin)
    {
        _logger = logger;
        _logger.Verbose("Starting RazerSdkReader...");
        _profiler = plugin.GetProfiler("Chroma Service");

        _enumNames = Enum.GetValues<RzDeviceType>().ToDictionary(t => t, t => $"Update {Enum.GetName(typeof(RzDeviceType), t) ?? throw new Exception()}");

        _reader = new();
        _reader.KeyboardUpdated += RazerEmulatorReaderOnKeyboardUpdated;
        _reader.MouseUpdated += RazerEmulatorReaderOnMouseUpdated;
        _reader.MousepadUpdated += RazerEmulatorReaderOnMousepadUpdated;
        _reader.KeypadUpdated += RazerEmulatorReaderOnKeypadUpdated;
        _reader.HeadsetUpdated += RazerEmulatorReaderOnHeadsetUpdated;
        _reader.ChromaLinkUpdated += RazerEmulatorReaderOnChromaLinkUpdated;
        _reader.AppDataUpdated += RazerEmulatorReaderOnAppDataUpdated;
        _reader.Start();
        _logger.Verbose("Started RazerSdkReader successfully");

        UpdateAppList(true);
    }

    private void RazerEmulatorReaderOnAppDataUpdated(object? sender, in ChromaAppData e)
    {
        UpdateAppListData(in e);
    }

    private void RazerEmulatorReaderOnChromaLinkUpdated(object? sender, in ChromaLink e)
    {
        UpdateMatrix(RzDeviceType.ChromaLink,in e);
    }

    private void RazerEmulatorReaderOnHeadsetUpdated(object? sender, in ChromaHeadset e)
    {
        UpdateMatrix(RzDeviceType.Headset,in e);
    }

    private void RazerEmulatorReaderOnKeypadUpdated(object? sender, in ChromaKeypad e)
    {
        UpdateMatrix(RzDeviceType.Keypad,in e);
    }

    private void RazerEmulatorReaderOnMousepadUpdated(object? sender, in ChromaMousepad e)
    {
        UpdateMatrix(RzDeviceType.Mousepad,in e);
    }

    private void RazerEmulatorReaderOnMouseUpdated(object? sender, in ChromaMouse e)
    {
        UpdateMatrix(RzDeviceType.Mouse,in e);
    }

    private void RazerEmulatorReaderOnKeyboardUpdated(object? sender, in ChromaKeyboard e)
    {
        UpdateMatrix(RzDeviceType.Keyboard, e);
    }

    internal void UpdateAppList(bool forced = false)
    {
        //TODO: Is this needed anymore?
        return;
    }

    private void UpdateMatrix<T>(RzDeviceType deviceType, in T data) where T : unmanaged, IColorProvider
    {
        var profilerName = _enumNames[deviceType];
        
        _profiler.StartMeasurement(profilerName);
        var matrix = Matrices.GetOrAdd(deviceType, static (id, t) => new SKColor[t.Height, t.Width], data);

        for (var i = 0; i < data.Height; i++)
        {
            for (var j = 0; j < data.Width; j++)
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                var clr = data.GetColor(i * data.Width + j);
                matrix[i, j] = new SKColor(clr.R, clr.G, clr.B);
            }
        }

        MatrixUpdated?.Invoke(this, deviceType);
        _profiler.StopMeasurement(profilerName);
    }

    private void UpdateAppListData(in ChromaAppData app)
    {
        Apps.Clear();
        Pids.Clear();
        CurrentAppId = (int?)app.CurrentAppId;
        CurrentApp = app.CurrentAppName;

        AppListUpdated?.Invoke(this, EventArgs.Empty);
        _logger.Verbose("Updated Chroma app list");
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