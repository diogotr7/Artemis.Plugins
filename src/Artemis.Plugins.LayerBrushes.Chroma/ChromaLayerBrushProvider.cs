using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Chroma;

public class ChromaLayerBrushProvider : LayerBrushProvider
{
    public override void Enable()
    {
        RegisterLayerBrushDescriptor<ChromaLayerBrush>("Chroma Grabber Layer", "Allows you to have Razer Chroma lighting on all devices.", "Robber");
    }

    public override void Disable()
    {
    }
}