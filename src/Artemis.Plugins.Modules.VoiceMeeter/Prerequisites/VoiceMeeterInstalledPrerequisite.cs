using Artemis.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Modules.VoiceMeeter.Prerequisites;

public class VoiceMeeterInstalledPrerequisite : PluginPrerequisite
{
    public override string Name => "VoiceMeeter Installed";

    public override string Description => "Checks if VoiceMeeter is installed and its files can be found.";

    public override List<PluginPrerequisiteAction> InstallActions { get; } = new();

    public override List<PluginPrerequisiteAction> UninstallActions { get; } = new();

    public override bool IsMet()
    {
        if (!TryGetVoiceMeeterDllPath(out var path))
            return false;

        if (NativeLibrary.Load(path) == IntPtr.Zero)
            return false;

        return true;
    }

    private static bool TryGetVoiceMeeterDllPath(out string path)
    {
        // Find current version from the registry
        const string INSTALLED_PROGRAMS = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}";
        const string INSTALLED_PROGRAMS32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}";
        const string UNINSTALL_KEY = "UninstallString";

        path = string.Empty;

        using var voiceMeeterSubKey = Registry.LocalMachine.OpenSubKey(INSTALLED_PROGRAMS) ?? Registry.LocalMachine.OpenSubKey(INSTALLED_PROGRAMS32);
        if (voiceMeeterSubKey == null)
            return false;

        var voiceMeeterPath = voiceMeeterSubKey.GetValue(UNINSTALL_KEY)?.ToString();
        if (string.IsNullOrWhiteSpace(voiceMeeterPath))
            return false;

        path = Path.Combine(Path.GetDirectoryName(voiceMeeterPath)!, "VoicemeeterRemote64.dll");
        return File.Exists(path);
    }
}
