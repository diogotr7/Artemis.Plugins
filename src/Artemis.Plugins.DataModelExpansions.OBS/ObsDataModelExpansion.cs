using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.OBS.DataModels;
using OBS.WebSocket.NET;
using OBS.WebSocket.NET.Types;

namespace Artemis.Plugins.DataModelExpansions.OBS
{
    public class ObsDataModelExpansion : DataModelExpansion<ObsDataModel>
    {
        private const string OBS_URI = "ws://127.0.0.1:4444";
        private const string OBS_PASSWORD = "";

        private ObsWebSocket _obs;

        public override void Enable()
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

        public override void Disable()
        {
            if (_obs != null)
            {
                _obs.Heartbeat -= UpdateHeartbeat;
                _obs.Disconnect();
            }
        }

        public override void Update(double deltaTime)
        {

        }

        private void UpdateHeartbeat(ObsWebSocket sender, Heartbeat heartbeat)
        {
            DataModel.CurrentProfile = heartbeat.CurrentProfile;
            DataModel.CurrentScene = heartbeat.CurrentScene;
            DataModel.Streaming = heartbeat.Streaming;
            DataModel.TotalStreamTime = heartbeat.TotalStreamTime;
            DataModel.TotalStreamBytes = heartbeat.TotalStreamBytes;
            DataModel.TotalStreamFrames = heartbeat.TotalStreamFrames;
            DataModel.Recording = heartbeat.Recording;
            DataModel.Paused = heartbeat.Paused;
            DataModel.TotalRecordTime = heartbeat.TotalRecordTime;
            DataModel.TotalTecordBytes = heartbeat.TotalTecordBytes;
            DataModel.TotalRecordFrames = heartbeat.TotalRecordFrames;
            DataModel.Stats = heartbeat.Stats;
        }
    }
}