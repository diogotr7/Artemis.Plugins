using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaLayerBrush : PerLedLayerBrush<MainPropertyGroup>
    {
        private RzSdkManager manager;
        private string currentApp;
        private readonly List<string> apps = new List<string>();
        private readonly List<int> pids = new List<int>();
        private readonly ConcurrentDictionary<LedId, SKColor> _colors = new ConcurrentDictionary<LedId, SKColor>();

        public override void EnableLayerBrush()
        {
            manager ??= new RzSdkManager()
            {
                AppListEnabled = true,
                MousepadEnabled = true,
                MouseEnabled = true,
                KeypadEnabled = true,
                KeyboardEnabled = true,
                HeadsetEnabled = true,
                ChromaLinkEnabled = true
            };
            manager.DataUpdated += OnDataUpdated;

            RzAppListDataProvider app = manager.GetDataProvider<RzAppListDataProvider>();

            UpdateAppListData(app);
        }

        public override void DisableLayerBrush()
        {
            manager.DataUpdated -= OnDataUpdated;
            //manager?.Dispose();
        }

        public override void Update(double deltaTime) { }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (currentApp is null)
                return SKColor.Empty;

            if (_colors.TryGetValue(led.RgbLed.Id, out SKColor clr))
                return clr;

            return SKColor.Empty;
        }

        private void OnDataUpdated(object sender, System.EventArgs e)
        {
            if (!(sender is AbstractDataProvider provider))
                return;

            provider.Update();

            if (provider is RzAppListDataProvider app)
            {
                UpdateAppListData(app);
            }
            else if (provider is AbstractColorDataProvider colorProvider)
            {
                if (!KeyMap.DeviceTypes.TryGetValue(colorProvider.GetType(), out Dictionary<(int Row, int Column), LedId> dict))
                    return;

                GridSize grid = colorProvider.Grids[0];
                for (int i = 0; i < grid.Height; i++)
                {
                    for (int j = 0; j < grid.Width; j++)
                    {
                        (byte r, byte g, byte b) = colorProvider.GetZoneColor((int)((i * grid.Width) + j));
                        if (dict.TryGetValue((i, j), out LedId ledId))
                        {
                            _colors[ledId] = new SKColor(r, g, b);
                        }
                    }
                }
            }
        }

        private void UpdateAppListData(RzAppListDataProvider app)
        {
            apps.Clear();
            pids.Clear();
            currentApp = app.CurrentAppExecutable;
            for (int i = 0; i < app.AppCount; i++)
            {
                apps.Add(app.GetExecutableName(i));
                pids.Add(app.GetPid(i));
            }
        }
    }
}