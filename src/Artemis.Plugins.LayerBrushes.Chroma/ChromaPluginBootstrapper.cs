using Artemis.Core;
using Artemis.Plugins.LayerBrushes.Chroma.Prerequisites;

namespace Artemis.Plugins.LayerBrushes.Chroma;

public class ChromaPluginBootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
        AddPluginPrerequisite(new ChromaSdkPluginPrerequisite(plugin));
    }
}