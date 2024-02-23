using Artemis.Core.Nodes;

namespace Artemis.Plugins.Nodes.Ping;

public class PingNodeProvider : NodeProvider
{
    public override void Enable()
    {
        RegisterNodeType<PingNode>();
    }

    public override void Disable()
    {
    }
}