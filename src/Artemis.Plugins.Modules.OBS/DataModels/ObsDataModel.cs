using Artemis.Core.Modules;
using OBSWebsocketDotNet.Types;

namespace Artemis.Plugins.Modules.OBS.DataModels;

public class ObsDataModel : DataModel
{
    public bool IsConnected { get; set; }
    public ObsStats? Stats { get; set; }
    public OutputStatus? StreamingStatus { get; set; }
    public RecordingStatus? RecordingStatus { get; set; }
    public GetProfileListInfo? ProfileListInfo { get; set; }
    public GetSceneListInfo? SceneListInfo { get; set; }
    
    
    public void Reset()
    {
        IsConnected = false;
        Stats = null;
        StreamingStatus = null;
        RecordingStatus = null;
        ProfileListInfo = null;
        SceneListInfo = null;
    }
}