using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;
using OBS.WebSocket.NET;
using OBS.WebSocket.NET.Types;

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
        public bool Paused { get; set; }
        public int TotalRecordTime { get; set; }
        public int TotalTecordBytes { get; set; }
        public int TotalRecordFrames { get; set; }
        public OBSStats Stats { get; set; }
    }
}