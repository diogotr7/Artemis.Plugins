namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

public class RazerSdkInfo
{
    public string Version { get; }
    
    public bool AppsEnabled { get; }
    
    public string[] PriorityList { get; }
    
    public RazerSdkInfo(string version, bool appsEnabled, string[] priorityList)
    {
        Version = version;
        AppsEnabled = appsEnabled;
        PriorityList = priorityList;
    }
}