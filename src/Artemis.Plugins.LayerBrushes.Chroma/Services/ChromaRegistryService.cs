using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Microsoft.Win32;
using Serilog;

namespace Artemis.Plugins.LayerBrushes.Chroma.Services;

/// <summary>
///  Service that handles data stored in the registry. 
/// </summary>
public sealed class ChromaRegistryService : IPluginService, IDisposable
{
    private readonly ILogger _logger;

    private readonly RegistryKey _razerSdkKey;
    private readonly RegistryKey _appsSdkKey;
    
    public ChromaRegistryService(ILogger logger)
    {
        _logger = logger;
        
        using var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        _razerSdkKey = localMachine.OpenSubKey(@"Software\Razer Chroma SDK") ?? throw new ArtemisPluginException("Razer Chroma SDK not installed");
        _appsSdkKey = _razerSdkKey.OpenSubKey("Apps") ?? throw new ArtemisPluginException("Razer Chroma SDK not installed");
    }

    public void SetPriorityList(string[] priorityList)
    {
        var priorityListString = string.Join(";", priorityList);
        _appsSdkKey.SetValue("PriorityList", priorityListString);
    }
    
    public RazerSdkInfo GetRazerSdkInfo()
    {
        var version = _razerSdkKey.GetValue("ProductVersion")?.ToString() 
                      ?? throw new ArtemisPluginException("Could not find version in Razer Chroma SDK registry key");
        var priorityList = _appsSdkKey.GetValue("PriorityList")?.ToString()?.Split(';') 
                           ?? throw new ArtemisPluginException("Could not find priority list in Razer Chroma SDK registry key");
        var enabledKey = _appsSdkKey.GetValue("Enable") as int? 
                         ?? throw new  ArtemisPluginException("Could not find enabled in Razer Chroma SDK registry key");
        var enabled = enabledKey == 1;
        
        return new RazerSdkInfo(version, enabled, priorityList);
    }
    
    public void EnsureValidPriorityList()
    {
        _logger.Verbose("Ensuring valid priority list");
        var priorityList = GetRazerSdkInfo().PriorityList;
        var artemisIndex = priorityList.IndexOf("Artemis.UI.Windows.exe");
        
        if (artemisIndex == -1)
        {
            _logger.Verbose("Artemis not in priority list, nothing to do");            
            return;
        }

        if (artemisIndex == priorityList.Length - 1)
        {
            _logger.Verbose("Artemis last in priority list, nothing to do");
            return;        
        }
        
        _logger.Verbose("Artemis not last in priority list, moving it to the end");
        // if artemis is not last, we need to move it to the end
        var newPriorityList = priorityList.Where((_, i) => i != artemisIndex).Append("Artemis.UI.Windows.exe").ToArray();

        _logger.Verbose("Setting new priority list: {newPriorityList}", newPriorityList);
        SetPriorityList(newPriorityList);
    }

    public void Dispose()
    {
        _razerSdkKey.Dispose();
        _appsSdkKey.Dispose();
    }
}