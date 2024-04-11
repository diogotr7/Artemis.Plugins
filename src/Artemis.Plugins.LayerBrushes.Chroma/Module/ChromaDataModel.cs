using Artemis.Core.Modules;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Artemis.Plugins.LayerBrushes.Chroma.Services;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

public class ChromaDataModel : DataModel
{
    public bool IsActive => Service.IsActive;
    public string? CurrentApplication => Service.CurrentApp;
    public string[] ApplicationList => Service.AppNames;
    public uint[] PidList => Service.AppIds;
    public string[] PriorityList { get; internal set; } = [];
    
    internal ChromaService Service { get; set; } = null!;
}

public class ChromaDeviceDataModel : DataModel
{
}

public class ChromaLedDataModel : DataModel
{
    public SKColor Color { get; set; }
}