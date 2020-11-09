using Artemis.Core;
using Artemis.Core.DefaultTypes;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticlePropertyGroup : LayerPropertyGroup
    {
        [PropertyGroupDescription(Description = "These define how and when particles spawn, and what properties a particle will have when it spawns.")]
        public ParticleSpawnPropertyGroup Spawn { get; set; }

        [PropertyGroupDescription]
        public ParticlePhysicsPropertyGroup Physics { get; set; }

        [PropertyGroupDescription]
        public ParticleColorPropertyGroup Color { get; set; }

        [PropertyGroupDescription]
        public ParticleTrailPropertyGroup Trail { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void EnableProperties()
        {

        }

        protected override void DisableProperties()
        {

        }
    }
}