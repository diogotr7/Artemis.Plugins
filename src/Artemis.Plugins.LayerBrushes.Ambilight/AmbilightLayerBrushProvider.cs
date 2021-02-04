using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Ambilight
{
    public class AmbilightLayerBrushProvider : LayerBrushProvider
    {
        public override void Enable()
        {
            RegisterLayerBrushDescriptor<AmbilightLayerBrush>("Ambilight", "", "Mirror");
        }

        public override void Disable()
        {
        }
    }
}