using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.OBS.DataModels;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.OBS
{
    public class ObsModule : Module<ObsDataModel>
    {
        private readonly IProcessMonitorService _processMonitorService;

        private const string OBS_URI = "ws://127.0.0.1:4444";
        private const string OBS_PASSWORD = "";
        private const string PROCESS_NAME = "obs64";

        private OBSWebsocket _obs;

        public ObsModule(IProcessMonitorService processMonitorService)
        {
            _processMonitorService = processMonitorService;
        }

        public override void Enable()
        {
            //subscribe to events
            _processMonitorService.ProcessStarted += OnProcessStarted;
            _processMonitorService.ProcessStopped += OnProcessStopped;

            //if obs is already running, try connecting
            if (_processMonitorService.GetRunningProcesses().Any(p => p.ProcessName == PROCESS_NAME))
                Task.Run(Connect);
        }

        public override void Disable()
        {
            _processMonitorService.ProcessStarted -= OnProcessStarted;
            _processMonitorService.ProcessStopped -= OnProcessStopped;

            Disconnect();
        }

        public override void Update(double deltaTime)
        {

        }

        private void OnProcessStarted(object sender, ProcessEventArgs e)
        {
            if (e.Process.ProcessName != PROCESS_NAME)
                return;

            Connect();
        }

        private void OnProcessStopped(object sender, ProcessEventArgs e)
        {
            if (e.Process.ProcessName != PROCESS_NAME)
                return;

            Disconnect();

            DataModel.Reset();
        }

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

        private void Disconnect()
        {
            if (_obs != null)
            {
                _obs.Heartbeat -= UpdateHeartbeat;
                _obs.Disconnect();
            }
        }
    }
}