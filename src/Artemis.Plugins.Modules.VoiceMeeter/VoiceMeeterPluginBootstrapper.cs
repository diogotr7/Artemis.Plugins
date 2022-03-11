﻿using Artemis.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.VoiceMeeter
{
    internal class VoiceMeeterPluginBootstrapper : PluginBootstrapper
    {
        public VoiceMeeterPluginBootstrapper()
        {
            if (!TryGetVoiceMeeterDllPath(out var path))
            {
                throw new ArtemisPluginException("Voicemeeter installation not found");
            }

            NativeLibrary.Load(path);
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

            path = Path.Combine(Path.GetDirectoryName(voiceMeeterPath), "VoicemeeterRemote64.dll");
            return File.Exists(path);
        }
    }
}