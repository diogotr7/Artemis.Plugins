using Artemis.Core.Services;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaPluginService : IPluginService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly RzSdkManager _manager;

        public event EventHandler<RzDeviceType> MatrixUpdated;
        public string CurrentApp { get; private set; }
        public List<string> Apps { get; } = new List<string>();
        public List<int> Pids { get; } = new List<int>();
        public ConcurrentDictionary<RzDeviceType, SKColor[,]> Matrices { get; } = new ConcurrentDictionary<RzDeviceType, SKColor[,]>();
        
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

            UpdateAppList();
        }

        internal void UpdateAppList()
        {
            _logger.Verbose("Running forced AppList update...");
            var applist = _manager.GetDataProvider<RzAppListDataProvider>();
            applist.Update();
            UpdateAppListData(applist);
        }

        private void OnDataUpdated(object sender, EventArgs e)
        {
            if (sender is not AbstractDataProvider provider)
                return;

            provider.Update();

            if (provider is RzAppListDataProvider app)
            {
                UpdateAppListData(app);
                _logger.Verbose("Updated AppList: CurrentApp: {currentApp} | Apps: {apps} | Pids: {pids}", CurrentApp ?? "None", Apps, Pids);
            }
            else if (provider is AbstractColorDataProvider colorProvider)
            {
                UpdateMatrix(colorProvider);
                _logger.Verbose("Updated {provider}. Zone zero: {color}", deviceTypeDict[colorProvider.GetType()], colorProvider.GetZoneColor(0));
            }
        }

        private void UpdateMatrix(AbstractColorDataProvider colorProvider)
        {
            RzDeviceType matrixDeviceType = deviceTypeDict[colorProvider.GetType()];
            GridSize grid = colorProvider.Grids[0];

            var matrix = Matrices.GetOrAdd(matrixDeviceType, new SKColor[grid.Height, grid.Width]);

            for (int i = 0; i < grid.Height; i++)
            {
                for (int j = 0; j < grid.Width; j++)
                {
                    (byte r, byte g, byte b) = colorProvider.GetZoneColor((int)((i * grid.Width) + j));
                    matrix[i, j] = new SKColor(r, g, b);
                }
            }

            MatrixUpdated?.Invoke(this, matrixDeviceType);
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

        private readonly Dictionary<Type, RzDeviceType> deviceTypeDict = new Dictionary<Type, RzDeviceType>
        {
            [typeof(RzMousepadDataProvider)] = RzDeviceType.Mousepad,
            [typeof(RzMouseDataProvider)] = RzDeviceType.Mouse,
            [typeof(RzKeypadDataProvider)] = RzDeviceType.Keypad,
            [typeof(RzKeyboardDataProvider)] = RzDeviceType.Keyboard,
            [typeof(RzHeadsetDataProvider)] = RzDeviceType.Headset,
            [typeof(RzChromaLinkDataProvider)] = RzDeviceType.ChromaLink
        };

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
