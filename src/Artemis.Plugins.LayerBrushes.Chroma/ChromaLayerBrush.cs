using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups;
using Serilog;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaLayerBrush : PerLedLayerBrush<MainPropertyGroup>
    {
        private readonly ChromaPluginService _chroma;
        public ChromaLayerBrush(ChromaPluginService chroma)
        {
            _chroma = chroma;
        }

        public override void EnableLayerBrush() { }

        public override void DisableLayerBrush() { }

        public override void Update(double deltaTime) { }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (_chroma.CurrentApp is null || _chroma.CurrentApp == "Artemis.UI.exe" || !_chroma.Colors.TryGetValue(led.RgbLed.Id, out SKColor clr))
                return SKColor.Empty;

            return clr;
        }
    }
}