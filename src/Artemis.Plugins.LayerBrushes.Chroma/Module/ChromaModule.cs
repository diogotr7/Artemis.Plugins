using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.LayerBrushes.Chroma.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

[PluginFeature(Name = "Chroma")]
public class ChromaModule : Module<ChromaDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

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
        CreateStructure();

        _chroma.MatrixUpdated += OnMatrixUpdated;
        _chroma.AppListUpdated += OnAppListUpdated;
        OnAppListUpdated(this, EventArgs.Empty);

        try
        {
            DataModel.PriorityList = _registry.GetRazerSdkInfo().PriorityList;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error setting priority list.");
            DataModel.PriorityList = Array.Empty<string>();
        }

        AddDefaultProfile(DefaultCategoryName.Games, Plugin.ResolveRelativePath("profile.zip"));
    }

    public override void Disable()
    {
        _chroma.MatrixUpdated -= OnMatrixUpdated;
        _chroma.AppListUpdated -= OnAppListUpdated;
    }

    public override void Update(double deltaTime)
    {
    }

    private void OnAppListUpdated(object? sender, EventArgs args)
    {
        DataModel.IsActive = _chroma.IsActive;
        DataModel.CurrentApplication = _chroma.CurrentApp;
        DataModel.ApplicationList = _chroma.AppNames.ToList();
        DataModel.PidList = _chroma.AppIds.Select(p => (int)p).ToList();
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
            for (var i = 0; i < map.Length; i++)
            {
                var ledId = map[i];
                if (ledId == LedId.Invalid)
                    continue;

                _colorsCache.Add(ledId, deviceDataModel.Value.AddDynamicChild<SKColor>(ledId.ToString(), default));
            }
        }
    }
}