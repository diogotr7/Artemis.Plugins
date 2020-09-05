using Artemis.Core;
using Artemis.Core.LayerBrushes;
using SkiaSharp;
using Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups;
using RazerSdkWrapper.Data;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaLayerBrush : PerLedLayerBrush<MainPropertyGroup>
    {
        private RzKeyboardDataProvider keyboard;
        private readonly SKColor[] _keyboardColors = new SKColor[22 * 6];

        public override void EnableLayerBrush()
        {
            keyboard = new RzKeyboardDataProvider();
        }

        public override void DisableLayerBrush()
        {
            keyboard?.Dispose();
        }

        public override void Update(double deltaTime)
        {
            keyboard.Update();

            for (var i = 0; i < keyboard.Grids[0].Height * keyboard.Grids[0].Width; i++)
            {
                var (r, g, b) = keyboard.GetZoneColor(i);
                _keyboardColors[i] = new SKColor(r, g, b);
            }
        }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (KeyMap.GenericKeyboard.TryGetValue(led.RgbLed.Id, out var position))
                return _keyboardColors[(position.Row * 22) + position.Column];

            return SKColor.Empty;
        }
    }
}