using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes;

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