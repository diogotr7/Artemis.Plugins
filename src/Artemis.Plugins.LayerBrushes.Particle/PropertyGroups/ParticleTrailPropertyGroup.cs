using Artemis.Core;
using Artemis.Core.DefaultTypes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticleTrailPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public BoolLayerProperty DrawTrail { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty TrailLength { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty TrailWidth { get; set; }

        protected override void PopulateDefaults()
        {
            DrawTrail.DefaultValue = false;
            TrailLength.DefaultValue = new SKPoint(0, 0);
            TrailWidth.DefaultValue = new SKPoint(0, 0);
        }

        protected override void EnableProperties() { }

        protected override void DisableProperties() { }
    }
}
