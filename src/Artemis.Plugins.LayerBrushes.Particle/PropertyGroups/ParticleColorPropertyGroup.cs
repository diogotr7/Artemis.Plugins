using Artemis.Core;
using SkiaSharp;

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
            Gradient.DefaultValue.Add(new ColorGradientStop(SKColors.Orange, 0));
            Gradient.DefaultValue.Add(new ColorGradientStop(SKColors.Red, 0.8f));
            Gradient.DefaultValue.Add(new ColorGradientStop(SKColors.Black, 1));
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

        private void ColorModeOnCurrentValueSet(object sender, LayerPropertyEventArgs e)
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
