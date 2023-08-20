﻿using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.LayerBrushes.Chroma.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Plugins.LayerBrushes.Chroma.Module;

[PluginFeature(Name = "Chroma Grabber")]
public class ChromaModule : Module<ChromaDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

    private readonly ILogger _logger;
    private readonly ChromaService _chroma;
    private readonly ChromaRegistryService _registry;
    private readonly object _lock = new();
    private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();
    private readonly Dictionary<RzDeviceType, DynamicChild<ChromaDeviceDataModel>> _deviceTypeCache = new();

    public ChromaModule(ChromaService chroma,ChromaRegistryService registry, ILogger logger)
    {
        _chroma = chroma;
        _registry = registry;
        _logger = logger;
    }

    public override void Enable()
    {
        _chroma.MatrixUpdated += UpdateMatrix;
        _chroma.AppListUpdated += UpdateAppList;
        UpdateAppList(null, EventArgs.Empty);
        try
        {
            DataModel.PriorityList = _registry.GetRazerSdkInfo().PriorityList;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error setting priority list.");
            DataModel.PriorityList = Array.Empty<string>();
        }
        
        AddDefaultProfile(DefaultCategoryName.Games, Plugin.ResolveRelativePath("Chroma.json"));
    }

    public override void Disable()
    {
        _chroma.MatrixUpdated -= UpdateMatrix;
        _chroma.AppListUpdated -= UpdateAppList;
    }

    public override void Update(double deltaTime)
    {

    }

    private void UpdateAppList(object? sender, EventArgs e)
    {
        DataModel.CurrentApplication = _chroma.CurrentApp;
        DataModel.ApplicationList = _chroma.Apps.ToList();
        DataModel.PidList = _chroma.Pids.ToList();
    }

    private void UpdateMatrix(object? sender, RzDeviceType rzDeviceType)
    {
        lock (_lock)
        {
            if (!_chroma.Matrices.TryGetValue(rzDeviceType, out var colors))
                return;

            if (!_deviceTypeCache.TryGetValue(rzDeviceType, out var deviceDataModel))
            {
                deviceDataModel = DataModel.AddDynamicChild(rzDeviceType.ToString(), new ChromaDeviceDataModel());
                _deviceTypeCache.Add(rzDeviceType, deviceDataModel);
            }

            for (var row = 0; row < colors.GetLength(0); row++)
            {
                for (var col = 0; col < colors.GetLength(1); col++)
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