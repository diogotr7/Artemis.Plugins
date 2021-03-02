using Artemis.Core.DataModelExpansions;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.DataModelExpansion
{
    public class ChromaDataModel : DataModel
    {
        public string CurrentApplication { get; internal set; }
        public List<string> ApplicationList { get; internal set; }
        public List<int> PidList { get; internal set; }
    }

    public class ChromaDeviceDataModel : DataModel { }

    public class ChromaLedDataModel : DataModel
    {
        public SKColor Color { get; set; }
    }
}