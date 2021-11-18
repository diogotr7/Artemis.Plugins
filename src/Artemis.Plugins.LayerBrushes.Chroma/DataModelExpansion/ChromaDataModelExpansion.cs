using Artemis.Core.Modules;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module
{
    public class ChromaDataModelExpansion : Module<ChromaDataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

        private readonly ILogger _logger;
        private readonly ChromaPluginService _chroma;
        private readonly object _lock = new();
        private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();
        private readonly Dictionary<RzDeviceType, DynamicChild<ChromaDeviceDataModel>> _deviceTypeCache = new();

        public ChromaDataModelExpansion(ChromaPluginService chroma, ILogger logger)
        {
            _chroma = chroma;
            _logger = logger;
        }

        public override void Enable()
        {
            _chroma.MatrixUpdated += UpdateMatrix;
            _chroma.AppListUpdated += UpdateAppList;
            try
            {
                DataModel.PriorityList = RazerChromaUtils.GetRazerPriorityList();
            }
            catch(Exception e)
            {
                _logger.Error("Error setting priority list.", e);
                DataModel.PriorityList = Array.Empty<string>();
            }
        }

        public override void Disable()
        {
            _chroma.MatrixUpdated -= UpdateMatrix;
            _chroma.AppListUpdated -= UpdateAppList;
        }

        public override void Update(double deltaTime)
        {

        }

        private void UpdateAppList(object sender, EventArgs e)
        {
            DataModel.CurrentApplication = _chroma.CurrentApp;
            DataModel.ApplicationList = _chroma.Apps;
            DataModel.PidList = _chroma.Pids;
        }

        private void UpdateMatrix(object sender, RzDeviceType rzDeviceType)
        {
            lock (_lock)
            {
                if (!_chroma.Matrices.TryGetValue(rzDeviceType, out SKColor[,] colors))
                    return;

                if (!_deviceTypeCache.TryGetValue(rzDeviceType, out var deviceDataModel))
                {
                    deviceDataModel = DataModel.AddDynamicChild(rzDeviceType.ToString(), new ChromaDeviceDataModel());
                    _deviceTypeCache.Add(rzDeviceType, deviceDataModel);
                }

                for (int row = 0; row < colors.GetLength(0); row++)
                {
                    for (int col = 0; col < colors.GetLength(1); col++)
                    {
                        var ledId = DefaultChromaLedMap.DeviceTypes[rzDeviceType][row, col];
                        if (ledId == LedId.Invalid)
                            continue;

                        if (!_colorsCache.TryGetValue(ledId, out var ledDataModel))
                        {
                            ledDataModel = deviceDataModel.Value.AddDynamicChild<SKColor>(ledId.ToString(), default);
                            _colorsCache.Add(ledId, ledDataModel);
                        }

                        ledDataModel.Value = colors[row, col];
                    }
                }
            }
        }
    }
}