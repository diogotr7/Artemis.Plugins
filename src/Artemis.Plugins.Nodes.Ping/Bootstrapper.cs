using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.Plugins.Nodes.Ping;

public class PingBootstrapper : PluginBootstrapper
{
    private readonly INodeService _nodeService;
    
    public PingBootstrapper(INodeService nodeService)
    {
        _nodeService = nodeService;
    }
    
    public override void OnPluginLoaded(Plugin plugin)
    {
        _nodeService.RegisterNodeType(plugin, typeof(PingNode));
    }

    public override void OnPluginEnabled(Plugin plugin)
    {
    }

    public override void OnPluginDisabled(Plugin plugin)
    {
    }
}