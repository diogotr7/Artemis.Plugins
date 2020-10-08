using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.OBS.DataModels;
using OBS.WebSocket.NET;
using OBS.WebSocket.NET.Types;
using System;
using System.Linq;

namespace Artemis.Plugins.DataModelExpansions.OBS
{
    public class ObsDataModelExpansion : DataModelExpansion<ObsDataModel>
    {
        private const string OBS_URI = "ws://127.0.0.1:4444";
        private const string OBS_PASSWORD = "";

        private ObsWebSocket _obs;

        public override void EnablePlugin()
        {
            _obs = new ObsWebSocket();

            try
            {
                _obs.Connect(OBS_URI, OBS_PASSWORD);
                _obs.Api.SetHeartbeat(true);
                _obs.Heartbeat += UpdateHeartbeat;

                DataModel.IsConnected = true;
            }
            catch
            {
                DataModel.IsConnected = false;
                //logger
            }
        }

        public override void DisablePlugin()
        {
            _obs.Heartbeat -= UpdateHeartbeat;
            _obs?.Disconnect();
        }

        public override void Update(double deltaTime)
        {

        }

        private void UpdateHeartbeat(ObsWebSocket sender, Heartbeat heatbeat)
        {
            DataModel.CurrentProfile = heatbeat.CurrentProfile;
            DataModel.CurrentScene = heatbeat.CurrentScene;
            DataModel.Streaming = heatbeat.Streaming;
            DataModel.TotalStreamTime = heatbeat.TotalStreamTime;
            DataModel.TotalStreamBytes = heatbeat.TotalStreamBytes;
            DataModel.TotalStreamFrames = heatbeat.TotalStreamFrames;
            DataModel.Recording = heatbeat.Recording;
            DataModel.Paused = heatbeat.Paused;
            DataModel.TotalRecordTime = heatbeat.TotalRecordTime;
            DataModel.TotalTecordBytes = heatbeat.TotalTecordBytes;
            DataModel.TotalRecordFrames = heatbeat.TotalRecordFrames;
            DataModel.Stats = heatbeat.Stats;
        }
    }
}