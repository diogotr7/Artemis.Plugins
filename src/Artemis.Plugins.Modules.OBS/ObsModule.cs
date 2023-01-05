using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.OBS.DataModels;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.OBS;

[PluginFeature(AlwaysEnabled = true, Name = "OBS")]
public class ObsModule : Module<ObsDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new() { new ProcessActivationRequirement(PROCESS_NAME) };

    private const string OBS_URI = "ws://127.0.0.1:4444";
    private const string OBS_PASSWORD = "";
    private const string PROCESS_NAME = "obs64";

    private OBSWebsocket _obs;

    public override void ModuleActivated(bool isOverride)
    {
        Connect();
    }

    public override void ModuleDeactivated(bool isOverride)
    {
        Disconnect();
        DataModel.Reset();
    }

    public override void Enable() { }

    public override void Disable() { }

    public override void Update(double deltaTime) { }

    private void Connect()
    {
        _obs = new OBSWebsocket();
        _obs.WSTimeout = TimeSpan.FromSeconds(2);
        _obs.Connected += OnObsConnected;

        try
        {
            _obs.Connect(OBS_URI, OBS_PASSWORD);
        }
        catch
        {
            DataModel.IsConnected = false;
            //logger
        }
    }

    private void Disconnect()
    {
        if (_obs != null)
        {
            _obs.Heartbeat -= UpdateHeartbeat;
            _obs.Disconnect();
        }
    }

    private void OnObsConnected(object sender, EventArgs e)
    {
        _obs.SetHeartbeat(true);
        _obs.Heartbeat += UpdateHeartbeat;

        DataModel.IsConnected = true;
    }

    private void UpdateHeartbeat(OBSWebsocket sender, Heartbeat heartbeat)
    {
        DataModel.CurrentProfile = heartbeat.CurrentProfile;
        DataModel.CurrentScene = heartbeat.CurrentScene;
        DataModel.Streaming = heartbeat.Streaming;
        DataModel.TotalStreamTime = heartbeat.totalStreamTime;
        DataModel.TotalStreamBytes = heartbeat.TotalStreamBytes;
        DataModel.TotalStreamFrames = heartbeat.TotalStreamFrames;
        DataModel.Recording = heartbeat.Recording;
        DataModel.TotalRecordTime = heartbeat.TotalRecordTime;
        DataModel.TotalTecordBytes = heartbeat.TotalTecordBytes;
        DataModel.TotalRecordFrames = heartbeat.TotalRecordFrames;
        DataModel.Stats = heartbeat.Stats;
    }
}