using Artemis.Core.Modules;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

public class ChromaDataModel : DataModel
{
    public string? CurrentApplication { get; internal set; } = string.Empty;
    public List<string> ApplicationList { get; internal set; } = new();
    public List<int> PidList { get; internal set; } = new();
    public string[] PriorityList { get; internal set; } = Array.Empty<string>();
}

public class ChromaDeviceDataModel : DataModel { }

public class ChromaLedDataModel : DataModel
{
    public SKColor Color { get; set; }
}