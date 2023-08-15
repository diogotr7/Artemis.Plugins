using Artemis.Core.Services;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public class ChromaService : IPluginService, IDisposable
{
    private readonly ILogger _logger;
    private readonly ChromaReader _reader;

    public event EventHandler<RzDeviceType>? MatrixUpdated;
    public event EventHandler? AppListUpdated;

    public int? CurrentAppId { get; private set; } = null;
    public string? CurrentApp { get; private set; } = null;
    public List<string> Apps { get; } = new();
    public List<int> Pids { get; } = new();
    public ConcurrentDictionary<RzDeviceType, SKColor[,]> Matrices { get; } = new();

    public ChromaService(ILogger logger)
    {
        _logger = logger;
        _logger.Verbose("Starting RazerSdkReader...");

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

    private void RazerEmulatorReaderOnAppDataUpdated(object? sender, ChromaAppData e)
    {
        UpdateAppListData(e);
    }

    private void RazerEmulatorReaderOnChromaLinkUpdated(object? sender, ChromaLink e)
    {
        UpdateMatrix(RzDeviceType.ChromaLink, e);
    }

    private void RazerEmulatorReaderOnHeadsetUpdated(object? sender, ChromaHeadset e)
    {
        UpdateMatrix(RzDeviceType.Headset, e);
    }

    private void RazerEmulatorReaderOnKeypadUpdated(object? sender, ChromaKeypad e)
    {
        UpdateMatrix(RzDeviceType.Keypad, e);
    }

    private void RazerEmulatorReaderOnMousepadUpdated(object? sender, ChromaMousepad e)
    {
        UpdateMatrix(RzDeviceType.Mousepad, e);
    }

    private void RazerEmulatorReaderOnMouseUpdated(object? sender, ChromaMouse e)
    {
        UpdateMatrix(RzDeviceType.Mouse, e);
    }

    private void RazerEmulatorReaderOnKeyboardUpdated(object? sender, ChromaKeyboard e)
    {
        UpdateMatrix(RzDeviceType.Keyboard, e);
    }

    internal void UpdateAppList(bool forced = false)
    {
        //TODO: Is this needed anymore?
        return;
    }

    private void UpdateMatrix<T>(RzDeviceType deviceType, T data) where T : IColorProvider
    {
        var matrix = Matrices.GetOrAdd(deviceType, static (id, g) => new SKColor[g.Height, g.Width], data);

        for (var i = 0; i < data.Height; i++)
        {
            for (var j = 0; j < data.Width; j++)
            {
                var clr = data.GetColor(i * data.Width + j);
                matrix[i, j] = new SKColor(clr.R, clr.G, clr.B);
            }
        }

        MatrixUpdated?.Invoke(this, deviceType);
    }

    private void UpdateAppListData(ChromaAppData app)
    {
        Apps.Clear();
        Pids.Clear();
        CurrentAppId = (int?)app.CurrentAppId;
        CurrentApp = null;
        for (var i = 0; i < app.AppCount; i++)
        {
            Apps.Add(app.AppInfo[i].AppName);
            Pids.Add((int)app.AppInfo[i].AppId);
            if (app.AppInfo[i].AppId == app.CurrentAppId)
                CurrentApp = app.AppInfo[i].AppName;
        }

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