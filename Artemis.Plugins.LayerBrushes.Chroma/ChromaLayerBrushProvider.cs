using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Chroma
{
    public class ChromaLayerBrushProvider : LayerBrushProvider
    {
        public override void EnablePlugin()
        {
            RegisterLayerBrushDescriptor<ChromaLayerBrush>("Razer Chroma Grabber", "", "ToyBrickPlus");
        }

        public override void DisablePlugin()
        {
        }
    }
}