using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes
{
    public class ChromaLayerBrush : PerLedLayerBrush<ChromaPropertyGroup>
    {
        private readonly ChromaPluginService _chroma;
        private readonly PluginSetting<Dictionary<RzDeviceType, LedId[,]>> _keyMapSetting;
        private readonly ConcurrentDictionary<LedId, SKColor> _colors;

        public ChromaLayerBrush(ChromaPluginService chroma, PluginSettings pluginSettings)
        {
            _chroma = chroma;
            _keyMapSetting = pluginSettings.GetSetting("ChromaLedArray", DefaultChromaLedMap.Clone());
            _colors = new();
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

        private void OnMatrixUpdated(object sender, RzDeviceType e)
        {
            SKColor[,] matrix = _chroma.Matrices[e];
            var dict = _keyMapSetting.Value[e];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    _colors[dict[i, j]] = matrix[i, j];
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

            if (_colors.TryGetValue(led.RgbLed.Id, out SKColor color))
                return color;

            //According to razer docs, chromaLink1 is the "catchall" ledId. If an LED doesn't have a mapping, use this color.
            if (Properties.UseDefaultLed.CurrentValue && _colors.TryGetValue(LedId.LedStripe1, out var chromaLink1))
                return chromaLink1;

            return SKColor.Empty;
        }
    }
}