using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.OBS.DataModels;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using Serilog;

namespace Artemis.Plugins.Modules.OBS;

[PluginFeature(AlwaysEnabled = true, Name = "OBS")]
public class ObsModule : Module<ObsDataModel>
{
    private readonly ILogger _logger;

    public override List<IModuleActivationRequirement> ActivationRequirements { get; } =
        [new ProcessActivationRequirement(PROCESS_NAME)];

    private const string OBS_URI = "ws://127.0.0.1:4455";
    private const string OBS_PASSWORD = "";
    private const string PROCESS_NAME = "obs64";

    private OBSWebsocket? _obs;

    public ObsModule(ILogger logger)
    {
        _logger = logger;
    }

    public override void ModuleActivated(bool isOverride)
    {
        Connect();
    }

    public override void ModuleDeactivated(bool isOverride)
    {
        Disconnect();
        DataModel.Reset();
    }

    public override void Enable()
    {
    }

    public override void Disable()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    private void Connect()
    {
        _obs = new OBSWebsocket();
        _obs.WSTimeout = TimeSpan.FromSeconds(2);
        _obs.Connected += OnObsConnected;

        try
        {
            _obs.ConnectAsync(OBS_URI, OBS_PASSWORD);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to connect to OBS");
            DataModel.IsConnected = false;
        }
    }

    private void Disconnect()
    {
        if (_obs != null)
        {
            Unsubscribe();
            _obs.Connected -= OnObsConnected;
            _obs.Disconnect();
        }
    }

    private void OnObsConnected(object? sender, EventArgs e)
    {
        if (_obs is null)
            return;

        Subscribe();
        DataModel.IsConnected = true;

        AddTimedUpdate(TimeSpan.FromSeconds(1), GetInitialData);
    }

    private void GetInitialData(double deltaTime)
    {
        if (_obs is null)
            return;

        try
        {
            DataModel.Stats = _obs.GetStats();
            DataModel.StreamingStatus = _obs.GetStreamStatus();
            DataModel.RecordingStatus = _obs.GetRecordStatus();
            DataModel.ProfileListInfo = _obs.GetProfileList();
            DataModel.SceneListInfo = _obs.GetSceneList();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to get initial data from OBS");
            DataModel.Reset();
        }
    }

    private void Subscribe()
    {
        if (_obs is null)
            return;
        
        //TODO
    }

    private void Unsubscribe()
    {
        if (_obs is null)
            return;

        //TODO
    }
}