using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Particle
{
    public class ParticleLayerBrushProvider : LayerBrushProvider
    {
        public override void EnablePlugin()
        {
            RegisterLayerBrushDescriptor<ParticleLayerBrush>("Particle", "A brush that simulates particles", "Grain");
        }

        public override void DisablePlugin()
        {
        }
    }
}