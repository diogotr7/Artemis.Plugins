using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Particle
{
    public class ParticleLayerBrushProvider : LayerBrushProvider
    {
        public override void Enable()
        {
            RegisterLayerBrushDescriptor<ParticleLayerBrush>("Particle", "A brush that simulates particles", "Grain");
        }

        public override void Disable()
        {
        }
    }
}