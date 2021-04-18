using Artemis.Core.DataModelExpansions;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModelExpansion : DataModelExpansion<ChromaDataModel>
    {
        private readonly ILogger _logger;
        private readonly ChromaPluginService _chroma;
        private readonly object _lock = new object();
        private readonly Dictionary<LedId, ChromaLedDataModel> _colorsCache = new();
        private readonly Dictionary<RzDeviceType, ChromaDeviceDataModel> _deviceTypeCache = new();

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

                if (!_deviceTypeCache.TryGetValue(rzDeviceType, out ChromaDeviceDataModel deviceDataModel))
                {
                    deviceDataModel = DataModel.AddDynamicChild(new ChromaDeviceDataModel(), rzDeviceType.ToString());
                    _deviceTypeCache.Add(rzDeviceType, deviceDataModel);
                }

                for (int row = 0; row < colors.GetLength(0); row++)
                {
                    for (int col = 0; col < colors.GetLength(1); col++)
                    {
                        var ledId = DefaultChromaLedMap.DeviceTypes[rzDeviceType][row, col];
                        if (ledId == LedId.Invalid)
                            continue;

                        if (!_colorsCache.TryGetValue(ledId, out ChromaLedDataModel ledDataModel))
                        {
                            ledDataModel = deviceDataModel.AddDynamicChild(new ChromaLedDataModel(), ledId.ToString());
                            _colorsCache.Add(ledId, ledDataModel);
                        }

                        ledDataModel.Color = colors[row, col];
                    }
                }
            }
        }
    }
}