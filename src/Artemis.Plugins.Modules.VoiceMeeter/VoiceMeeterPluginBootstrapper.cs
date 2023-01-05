using Artemis.Core;
using Artemis.Plugins.Modules.VoiceMeeter.Prerequisites;

namespace Artemis.Plugins.Modules.VoiceMeeter;

public class VoiceMeeterPluginBootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        AddPluginPrerequisite(new VoiceMeeterInstalledPrerequisite());
    }
}
