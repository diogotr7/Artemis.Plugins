using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaLayerBrush : PerLedLayerBrush<MainPropertyGroup>
    {
        private RzSdkManager manager;
        private readonly List<string> apps = new List<string>();
        private readonly List<int> pids = new List<int>();
        private readonly SKColor[] _keyboardColors = new SKColor[22 * 6];

        public override void EnableLayerBrush()
        {
            manager = new RzSdkManager()
            {
                KeyboardEnabled = true,
                AppListEnabled = true
            };
            manager.DataUpdated += OnDataUpdated;
        }

        private void OnDataUpdated(object sender, System.EventArgs e)
        {
            switch (sender)
            {
                case RzKeyboardDataProvider keyboard:
                    keyboard.Update();

                    for (var i = 0; i < keyboard.Grids[0].Height * keyboard.Grids[0].Width; i++)
                    {
                        var (r, g, b) = keyboard.GetZoneColor(i);
                        _keyboardColors[i] = new SKColor(r, g, b);
                    }

                    break;
                case RzAppListDataProvider app:
                    app.Update();
                    apps.Clear();
                    pids.Clear();

                    for (int i = 0; i < app.AppCount; i++)
                    {
                        apps.Add(app.GetExecutableName(i));
                        pids.Add(app.GetPid(i));
                    }

                    break;
            }
        }

        public override void DisableLayerBrush()
        {
            manager.DataUpdated -= OnDataUpdated;
            manager?.Dispose();
        }

        public override void Update(double deltaTime) { }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (_keyboardColors.All(c => c.Red == 0 && c.Green == 0 & c.Blue == 0))
                return SKColor.Empty;

            if (KeyMap.GenericKeyboard.TryGetValue(led.RgbLed.Id, out var position))
                return _keyboardColors[(position.Row * 22) + position.Column];

            return SKColor.Empty;
        }
    }
}