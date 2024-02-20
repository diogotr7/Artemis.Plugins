using System;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.Services;
using Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes.PropertyGroups;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes;

public class ChromaLayerBrush : PerLedLayerBrush<ChromaPropertyGroup>
{
    private readonly ChromaService _chroma;
    private readonly Dictionary<LedId, SKColor> _colors;
    private readonly object _lock;
    private bool _shouldRender;
    private bool _playingOverwatch;

    public ChromaLayerBrush(ChromaService chroma)
    {
        _chroma = chroma;
        _colors = new();
        _lock = new();
    }

    public override void EnableLayerBrush()
    {
        _chroma.MatrixUpdated += OnMatrixUpdated;
        _chroma.AppListUpdated += OnAppListUpdated;
        OnAppListUpdated(this, EventArgs.Empty);
    }

    private void OnAppListUpdated(object? sender, EventArgs e)
    {
        _shouldRender = _chroma.IsActive;
        _playingOverwatch = _chroma.CurrentApp == "Overwatch.exe";
    }

    public override void DisableLayerBrush()
    {
        _chroma.MatrixUpdated -= OnMatrixUpdated;
        _chroma.AppListUpdated -= OnAppListUpdated;
    }

    private void OnMatrixUpdated(object? sender, RzDeviceType deviceType)
    {
        var dict = DefaultChromaLedMap.GetDeviceMap(deviceType);
        var matrix = _chroma.Matrices[(int) deviceType];

        lock (_lock)
        {
            for (var i = 0; i < matrix.Length; i++)
            {
                _colors[dict[i]] = matrix[i];
            }
        }
    }

    public override void Update(double deltaTime)
    {
    }

    public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
    {
        if (!_shouldRender)
            return SKColor.Empty;

        lock (_lock)
        {
            if (_colors.TryGetValue(led.RgbLed.Id, out var color))
                return ProcessColor(color);

            //According to razer docs, chromaLink1 is the "catchall" ledId. If an LED doesn't have a mapping, use this color.
            if (Properties.UseDefaultLed && _colors.TryGetValue(LedId.LedStripe1, out var chromaLink1))
                return ProcessColor(chromaLink1);
        }

        return SKColor.Empty;
    }

    private SKColor ProcessColor(SKColor color)
    {
        if (Properties.TransparentBlack && color == SKColors.Black)
            return SKColor.Empty;

        if (_playingOverwatch && Properties.OverwatchEnhanceColors && OverwatchColorCorrection.ColorMap.TryGetValue(color, out var correctedColor))
            return correctedColor;

        return color;
    }
}