using Artemis.Core.Modules;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

public class ChromaDataModel : DataModel
{
    public bool IsActive { get; set; }
    public string? CurrentApplication { get; set; }
    public List<string> ApplicationList { get; internal set; } = new();
    public List<int> PidList { get; internal set; } = new();
    public string[] PriorityList { get; internal set; } = Array.Empty<string>();
}

public class ChromaDeviceDataModel : DataModel { }

public class ChromaLedDataModel : DataModel
{
    public SKColor Color { get; set; }
}