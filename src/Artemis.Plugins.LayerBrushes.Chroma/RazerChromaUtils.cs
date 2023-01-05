using Microsoft.Win32;
using System;

namespace Artemis.Plugins.LayerBrushes.Chroma;

public static class RazerChromaUtils
{
    public static string[] GetRazerPriorityList()
    {
        var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        var razerChroma = localMachine.OpenSubKey(@"Software\Razer Chroma SDK");
        if (razerChroma == null)
            throw new Exception("Razer Chroma SDK not installed");
        
        var razerChromaPriorityList = razerChroma.GetValue("PriorityList");
        if (razerChromaPriorityList is not null and string s)
        {
            return s.Split(';');
        }

        var apps = razerChroma.OpenSubKey("Apps");
        if (apps != null)
        {
            var appsPriorityList = apps.GetValue("PriorityList");
            if (appsPriorityList is string st)
            {
                return st.Split(';');
            }
        }

        return Array.Empty<string>();
    }
}
