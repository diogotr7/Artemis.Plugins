using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.Plugins.Nodes.Ping;

public class PingBootstrapper : PluginBootstrapper
{
    public override void OnPluginLoaded(Plugin plugin)
    {
    }

    public override void OnPluginEnabled(Plugin plugin)
    {
        var nodeService = plugin.Resolve<INodeService>();
        nodeService.RegisterNodeType(plugin, typeof(PingNode));
    }

    public override void OnPluginDisabled(Plugin plugin)
    {
    }
}