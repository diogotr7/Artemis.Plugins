using Artemis.Core.DataModelExpansions;
using OBSWebsocketDotNet.Types;

namespace Artemis.Plugins.DataModelExpansions.OBS.DataModels
{
    public class ObsDataModel : DataModel
    {
        public bool IsConnected { get; set; }
        public string CurrentProfile { get; set; }
        public string CurrentScene { get; set; }
        public bool Streaming { get; set; }
        public int TotalStreamTime { get; set; }
        public ulong TotalStreamBytes { get; set; }
        public ulong TotalStreamFrames { get; set; }
        public bool Recording { get; set; }
        public int TotalRecordTime { get; set; }
        public int TotalTecordBytes { get; set; }
        public int TotalRecordFrames { get; set; }
        public OBSStats Stats { get; set; }

        internal void Reset()
        {
            IsConnected = default;
            CurrentProfile = default;
            CurrentScene = default;
            Streaming = default;
            TotalStreamTime = default;
            TotalStreamBytes = default;
            TotalStreamFrames = default;
            Recording = default;
            TotalRecordTime = default;
            TotalTecordBytes = default;
            TotalRecordFrames = default;
            Stats = new OBSStats();
        }
    }
}