using Artemis.Core;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticlePhysicsPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public SKPointLayerProperty Acceleration { get; set; }

        [PropertyDescription(MinInputValue = 0f, MaxInputValue = 1f, InputStepSize = 0.01f)]
        public SKPointLayerProperty Drag { get; set; }

        [PropertyDescription]
        public FloatLayerProperty DeltaSize { get; set; }

        protected override void DisableProperties()
        {
            Acceleration.DefaultValue = new SKPoint(0, 0);
            Drag.DefaultValue = new SKPoint(0, 0);
            DeltaSize.DefaultValue = -10;
        }

        protected override void EnableProperties() { }

        protected override void PopulateDefaults() { }
    }
}
