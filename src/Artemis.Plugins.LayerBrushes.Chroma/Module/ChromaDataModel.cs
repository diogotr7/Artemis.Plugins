using Artemis.Core.Modules;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

public class ChromaDataModel : DataModel
{
    public string CurrentApplication { get; internal set; }
    public List<string> ApplicationList { get; internal set; }
    public List<int> PidList { get; internal set; }
    public string[] PriorityList { get; internal set; }
}

public class ChromaDeviceDataModel : DataModel { }

public class ChromaLedDataModel : DataModel
{
    public SKColor Color { get; set; }
}