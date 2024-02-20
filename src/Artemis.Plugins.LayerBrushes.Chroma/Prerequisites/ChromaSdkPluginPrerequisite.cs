using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Microsoft.Win32;

namespace Artemis.Plugins.LayerBrushes.Chroma.Prerequisites;

public class ChromaSdkPluginPrerequisite : PluginPrerequisite
{
    public override string Name => "Razer Chroma SDK Core";

    public override string Description => "Services needed for Chroma games to send lighting";

    public override List<PluginPrerequisiteAction> InstallActions { get; }

    public override List<PluginPrerequisiteAction> UninstallActions { get; }

    public override bool IsMet()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Razer Chroma SDK");

        return key != null;
    }

    public ChromaSdkPluginPrerequisite()
    {
        var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);
        var uninstallerPath = Path.Combine(programFilesX86, "Razer Chroma SDK", "Razer_Chroma_SDK_Uninstaller.exe");

        InstallActions = new List<PluginPrerequisiteAction>
        {
            new DownloadAndInstallChromaSdkAction(),
        };

        UninstallActions = new List<PluginPrerequisiteAction>
        {
            new ExecuteFileAction("Uninstall Chroma SDK", uninstallerPath, elevate: true, arguments: "/S")
        };
    }
}