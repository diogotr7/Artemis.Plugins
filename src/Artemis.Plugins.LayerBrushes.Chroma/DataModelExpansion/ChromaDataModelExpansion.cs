using Artemis.Core.DataModelExpansions;
using Serilog;
using SkiaSharp;
using System;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModelExpansion : DataModelExpansion<ChromaDataModel>
    {
        private readonly ILogger _logger;
        private readonly ChromaPluginService _chroma;
        private readonly object _lock = new object();

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

                var deviceDataModel = DataModel.DynamicChild<ChromaDeviceDataModel>(rzDeviceType.ToString())
                                    ?? DataModel.AddDynamicChild(new ChromaDeviceDataModel(), rzDeviceType.ToString());

                for (int row = 0; row < colors.GetLength(0); row++)
                {
                    for (int col = 0; col < colors.GetLength(1); col++)
                    {
                        var ledId = DefaultChromaLedMap.DeviceTypes[rzDeviceType][row, col];
                        if (ledId == RGB.NET.Core.LedId.Invalid)
                            continue;

                        var chromaKeyDataModel = deviceDataModel.DynamicChild<ChromaLedDataModel>(ledId.ToString());
                        if (chromaKeyDataModel != null)
                            chromaKeyDataModel.Color = colors[row, col];
                        else
                            deviceDataModel.AddDynamicChild(new ChromaLedDataModel { Color = colors[row, col] }, ledId.ToString());
                    }
                }
            }
        }
    }
}