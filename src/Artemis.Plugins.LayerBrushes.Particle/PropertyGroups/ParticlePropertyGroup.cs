using Artemis.Core;
using Artemis.Core.DefaultTypes;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticlePropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public BoolLayerProperty SpawnEnabled { get; set; }

        [PropertyDescription]
        public EnumLayerProperty<SpawnPosition> SpawnPosition { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public IntRangeLayerProperty SpawnAmountRange { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public FloatRangeLayerProperty SpawnTimeRange { get; set; }

        [PropertyDescription(Description = "Lifetime", MinInputValue = 0)]
        public FloatRangeLayerProperty MaxLifetimeRange { get; set; }

        [PropertyDescription]
        public FloatRangeLayerProperty InitialXVelocityRange { get; set; }

        [PropertyDescription]
        public FloatRangeLayerProperty InitialYVelocityRange { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public FloatRangeLayerProperty SizeRange { get; set; }

        [PropertyDescription]
        public FloatLayerProperty DeltaSize { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty Acceleration { get; set; }

        [PropertyDescription(MinInputValue = 0, MaxInputValue = 1)]
        public SKPointLayerProperty Drag { get; set; }

        [PropertyDescription]
        public ColorGradientLayerProperty Gradient { get; set; }

        protected override void PopulateDefaults()
        {
            SpawnEnabled.DefaultValue = true;
            SpawnTimeRange.DefaultValue = new FloatRange(0.5f, 1f);
            SpawnAmountRange.DefaultValue = new IntRange(1, 10);
            InitialYVelocityRange.DefaultValue = new FloatRange(0.5f, 1);
            InitialXVelocityRange.DefaultValue = new FloatRange(0, 0);
            MaxLifetimeRange.DefaultValue = new FloatRange(0, 5);
            Acceleration.DefaultValue = new SKPoint(0, 0);
            Drag.DefaultValue = new SKPoint(0, 0);
            SizeRange.DefaultValue = new FloatRange(6, 12);
            DeltaSize.DefaultValue = 0;
            SpawnPosition.DefaultValue = PropertyGroups.SpawnPosition.TopEdge;
            Gradient.DefaultValue = ColorGradient.GetUnicornBarf();
        }

        protected override void EnableProperties()
        {

        }

        protected override void DisableProperties()
        {

        }
    }

    public enum SpawnPosition
    {
        TopEdge,
        RightEdge,
        BottomEdge,
        LeftEdge,
        Random,
        Center
    }

    public enum ColorMode
    {
        LifetimeBased,
        Static
    }
}