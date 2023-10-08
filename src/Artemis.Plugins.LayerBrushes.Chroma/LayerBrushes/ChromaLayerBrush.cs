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
    private readonly HashSet<RGBDeviceType> _perLedDeviceTypes;
    private readonly object _lock;
    private bool _shouldRender;
    private bool _playingOverwatch;

    public ChromaLayerBrush(ChromaService chroma)
    {
        _chroma = chroma;
        _colors = new();
        _lock = new();
        _perLedDeviceTypes = new()
        {
            RGBDeviceType.Keyboard,
            RGBDeviceType.Mouse,
            RGBDeviceType.Mousepad,
            RGBDeviceType.Headset
        };
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

    private void OnMatrixUpdated(object? sender, MatrixUpdatedEventArgs args)
    {
        if (args.DeviceType == RzDeviceType.ChromaLink)
        {
            for (var i = 0; i < args.Matrix.GetLength(0); i++)
            {
                for (var j = 0; j < args.Matrix.GetLength(1); j++)
                {
                    _chromaLink[i * 5 + j] = args.Matrix[i, j];
                }
            }
            
            return;
        }
        
        var matrix = args.Matrix;
        var dict = DefaultChromaLedMap.DeviceTypes[args.DeviceType];

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

    public override void Update(double deltaTime) { }

    public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
    {
        if (!_shouldRender)
            return SKColor.Empty;

        lock (_lock)
        {
            if (!_perLedDeviceTypes.Contains(led.Device.DeviceType))
                return GetLedForDevice(led);
            
            if (_colors.TryGetValue(led.RgbLed.Id, out var color))
                return ProcessColor(color);
            
            if (Properties.UseDefaultLed && TryGetDefaultLed(led, out var defaultLed))
                return ProcessColor(defaultLed);
            
            //According to razer docs, chromaLink1 is the "catchall" ledId. If an LED doesn't have a mapping, use this color.
            if (Properties.UseDefaultLed && _colors.TryGetValue(LedId.LedStripe1, out var chromaLink1))
                return ProcessColor(chromaLink1);
        }

        return SKColor.Empty;
    }

    /// <summary>
    ///     Gets the default color for the provided LED based on the device type
    /// </summary>
    private bool TryGetDefaultLed(ArtemisLed led, out SKColor color) => led.Device.DeviceType switch
    {
        RGBDeviceType.Keyboard => _colors.TryGetValue(LedId.Keyboard_Space, out color),
        RGBDeviceType.Mouse => _colors.TryGetValue(LedId.Mouse1, out color),
        RGBDeviceType.Mousepad => _colors.TryGetValue(LedId.Mousepad1, out color),
        RGBDeviceType.Headset => _colors.TryGetValue(LedId.Headset1, out color),
        RGBDeviceType.Keypad => _colors.TryGetValue(LedId.Keypad1, out color),
        _ => _colors.TryGetValue(LedId.LedStripe1, out color)
    };

    private SKColor ProcessColor(SKColor color)
    {
        if (Properties.TransparentBlack && color == SKColors.Black)
            return SKColor.Empty;
        
        if (_playingOverwatch && Properties.OverwatchEnhanceColors && OverwatchColorCorrection.ColorMap.TryGetValue(color, out var correctedColor))
            return correctedColor;

        return color;
    }

    private readonly SKColor[] _chromaLink = new SKColor[5];
    private SKColor GetLedForDevice(ArtemisLed led)
    {
        //if we get here, the device should be colored with animated chroma link
        var ledIndex = led.Device.Leds.IndexOf(led);
        
        //chroma link has 4 LEDs
        var chromaLinkIndex = ledIndex % 5;
        
        return _chromaLink[chromaLinkIndex];
    }
}