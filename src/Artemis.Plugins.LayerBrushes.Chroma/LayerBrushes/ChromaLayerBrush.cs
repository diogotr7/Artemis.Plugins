using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.Services;
using Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes.PropertyGroups;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes;

public class ChromaLayerBrush : PerLedLayerBrush<ChromaPropertyGroup>
{
    private readonly ChromaService _chroma;
    private readonly PluginSetting<Dictionary<RzDeviceType, LedId[,]>> _keyMapSetting;
    private readonly Dictionary<LedId, SKColor> _colors;
    private readonly object _lock;

    public ChromaLayerBrush(ChromaService chroma, PluginSettings pluginSettings)
    {
        _chroma = chroma;
        _keyMapSetting = pluginSettings.GetSetting("ChromaKeymap", DefaultChromaLedMap.Clone());
        _colors = new();
        _lock = new();
    }

    private double forceRefreshAppListTimer;

    public override void EnableLayerBrush()
    {
        _chroma.MatrixUpdated += OnMatrixUpdated;
    }

    public override void DisableLayerBrush()
    {
        _chroma.MatrixUpdated -= OnMatrixUpdated;
    }

    private void OnMatrixUpdated(object? sender, RzDeviceType e)
    {
        SKColor[,] matrix = _chroma.Matrices[e];
        var dict = _keyMapSetting.Value[e];

        lock (_lock)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    _colors[dict[i, j]] = matrix[i, j];
                }
            }
        }
    }

    public override void Update(double deltaTime)
    {
        //when a chroma app is closed, the applist update event isn't fired for some reason.
        //to prevent this brush being left hanging and painting everything black,
        //we force update this every 5 seconds. This makes the brush paint transparent
        //if no chroma apps are running.
        if (forceRefreshAppListTimer < 5d)
        {
            forceRefreshAppListTimer += deltaTime;
        }
        else
        {
            _chroma.UpdateAppList();
            forceRefreshAppListTimer = 0;
        }
    }

    public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
    {
        if (string.IsNullOrWhiteSpace(_chroma.CurrentApp) || _chroma.CurrentApp.Contains("Artemis.UI"))
            return SKColor.Empty;

        lock (_lock)
        {
            if (_colors.TryGetValue(led.RgbLed.Id, out var color))
                return ProcessColor(color);
            
            //According to razer docs, chromaLink1 is the "catchall" ledId. If an LED doesn't have a mapping, use this color.
            if (Properties.UseDefaultLed.CurrentValue && _colors.TryGetValue(LedId.LedStripe1, out var chromaLink1))
                return ProcessColor(chromaLink1);
        }

        return SKColor.Empty;
    }

    private SKColor ProcessColor(SKColor color)
    {
        if (Properties.TransparentBlack.CurrentValue && color == SKColors.Black)
            return SKColor.Empty;

        return color;
    }
}