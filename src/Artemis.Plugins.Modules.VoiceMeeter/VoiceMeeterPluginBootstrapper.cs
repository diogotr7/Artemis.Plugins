using Artemis.Core;
using Artemis.Plugins.Modules.VoiceMeeter.Prerequisites;
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
    public class VoiceMeeterPluginBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            AddPluginPrerequisite(new VoiceMeeterInstalledPrerequisite());
        }
    }
}
