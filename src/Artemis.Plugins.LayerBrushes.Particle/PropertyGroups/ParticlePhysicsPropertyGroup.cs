using Artemis.Core;
using Artemis.Core.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticlePhysicsPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public SKPointLayerProperty Acceleration { get; set; }

        [PropertyDescription(MinInputValue = 0, MaxInputValue = 1, InputStepSize = 0.01f)]
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
