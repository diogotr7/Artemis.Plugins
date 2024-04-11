using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.LayerBrushes.Chroma.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

[PluginFeature(Name = "Chroma")]
public class ChromaModule : Module<ChromaDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = [];

    private readonly ILogger _logger;
    private readonly ChromaService _chroma;
    private readonly ChromaRegistryService _registry;
    private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();

    public ChromaModule(ChromaService chroma, ChromaRegistryService registry, ILogger logger)
    {
        _chroma = chroma;
        _registry = registry;
        _logger = logger;
    }

    public override void Enable()
    {
        DataModel.Service = _chroma;
        
        CreateStructure();

        _chroma.MatrixUpdated += OnMatrixUpdated;

        try
        {
            DataModel.PriorityList = _registry.GetRazerSdkInfo().PriorityList;
            var artemis = DataModel.PriorityList.IndexOf("Artemis.UI.Windows.exe");
            if (artemis != -1 && artemis != DataModel.PriorityList.Length - 1)
            {
                _logger.Error("Artemis is not the last item in the Razer Chroma SDK priority list, the Chroma grabber may not work correctly.");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error setting priority list.");
            DataModel.PriorityList = [];
        }

        AddDefaultProfile(DefaultCategoryName.Games, Plugin.ResolveRelativePath("profile.zip"));
    }

    public override void Disable()
    {
        _chroma.MatrixUpdated -= OnMatrixUpdated;
    }

    public override void Update(double deltaTime)
    {
    }

    private void OnMatrixUpdated(object? sender, MatrixUpdatedEventArgs e)
    {
        var (deviceType, colors) = e;

        var map = DefaultChromaLedMap.GetDeviceMap(deviceType);

        for (var i = 0; i < colors.Length; i++)
        {
            var ledId = map[i];
            if (ledId == LedId.Invalid)
                continue;

            _colorsCache[ledId].Value = colors[i];
        }
    }

    private void CreateStructure()
    {
        DataModel.ClearDynamicChildren();
        foreach (var rzDeviceType in Enum.GetValues<RzDeviceType>())
        {
            var deviceDataModel = DataModel.AddDynamicChild(rzDeviceType.ToStringFast(), new ChromaDeviceDataModel());

            var map = DefaultChromaLedMap.GetDeviceMap(rzDeviceType);
            foreach (var ledId in map)
            {
                if (ledId == LedId.Invalid)
                    continue;

                _colorsCache.Add(ledId, deviceDataModel.Value.AddDynamicChild<SKColor>(ledId.ToString(), default));
            }
        }
    }
}