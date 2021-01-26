using Artemis.Core.Services;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaPluginService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly RzSdkManager _manager;
        public string CurrentApp { get; private set; }
        public List<string> Apps { get; } = new List<string>();
        public List<int> Pids { get; } = new List<int>();
        public ConcurrentDictionary<LedId, SKColor> Colors { get; } = new ConcurrentDictionary<LedId, SKColor>();

        public ChromaPluginService(ILogger logger)
        {
            _logger = logger;

            _logger.Verbose("Starting RzSDKManager...");
            _manager = new RzSdkManager()
            {
                AppListEnabled = true,
                MousepadEnabled = true,
                MouseEnabled = true,
                KeypadEnabled = true,
                KeyboardEnabled = true,
                HeadsetEnabled = true,
                ChromaLinkEnabled = true
            };
            _logger.Verbose("Started RzSdkManager successfully");
            _manager.DataUpdated += OnDataUpdated;

            RzAppListDataProvider app = _manager.GetDataProvider<RzAppListDataProvider>();
            UpdateAppListData(app);
        }

        private void OnDataUpdated(object sender, System.EventArgs e)
        {
            if (sender is not AbstractDataProvider provider)
                return;

            provider.Update();

            if (provider is RzAppListDataProvider app)
            {
                UpdateAppListData(app);
                _logger.Verbose($"Updated AppList: CurrentApp: {CurrentApp ?? "None"} |  Apps: {string.Join(',', Apps)} | Pids: {string.Join(',', Pids)}");
            }
            else if (provider is AbstractColorDataProvider colorProvider)
            {
                if (!KeyMap.DeviceTypes.TryGetValue(colorProvider.GetType(), out Dictionary<(int Row, int Column), LedId> dict))
                    return;

                _logger.Verbose($"Updated {provider.GetType()}: {colorProvider.GetZoneColor(0)}");
                GridSize grid = colorProvider.Grids[0];
                for (int i = 0; i < grid.Height; i++)
                {
                    for (int j = 0; j < grid.Width; j++)
                    {
                        (byte r, byte g, byte b) = colorProvider.GetZoneColor((int)((i * grid.Width) + j));
                        if (dict.TryGetValue((i, j), out LedId ledId))
                        {
                            Colors[ledId] = new SKColor(r, g, b);
                        }
                    }
                }
            }
        }

        private void UpdateAppListData(RzAppListDataProvider app)
        {
            Apps.Clear();
            Pids.Clear();
            CurrentApp = app.CurrentAppExecutable;
            for (int i = 0; i < app.AppCount; i++)
            {
                Apps.Add(app.GetExecutableName(i));
                Pids.Add(app.GetPid(i));
            }
        }

        #region IDisposable
        public void Dispose()
        {
            _manager.DataUpdated -= OnDataUpdated;
            //TODO: disposing this throws this:
            //System.ApplicationException: Object synchronization method was called from an unsynchronized block of code.
            //idk what to do about it
            //_manager?.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
