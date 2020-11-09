using Artemis.Core;
using Artemis.Core.DefaultTypes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticleColorPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public EnumLayerProperty<ParticleColorMode> ColorMode { get; set; }

        [PropertyDescription]
        public SKColorLayerProperty Color { get; set; }

        [PropertyDescription]
        public ColorGradientLayerProperty Gradient { get; set; }

        protected override void PopulateDefaults()
        {
            ColorMode.DefaultValue = ParticleColorMode.Lifetime;
            Gradient.DefaultValue = new ColorGradient();
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Orange, 0));
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Red, 0.8f));
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Black, 1));
        }

        protected override void EnableProperties()
        {
            ColorMode.CurrentValueSet += ColorModeOnCurrentValueSet;
            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
            ColorMode.CurrentValueSet -= ColorModeOnCurrentValueSet;
        }

        private void ColorModeOnCurrentValueSet(object sender, LayerPropertyEventArgs<ParticleColorMode> e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            Color.IsHidden = ColorMode.BaseValue != ParticleColorMode.Static;
            Gradient.IsHidden = ColorMode.BaseValue == ParticleColorMode.Static;
        }
    }
}
