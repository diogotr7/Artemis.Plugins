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

        [PropertyDescription]
        public BoolLayerProperty DespawnOutOfBounds { get; set; }

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

        [PropertyDescription]
        public BoolLayerProperty DrawTrail { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty TrailLength { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty TrailWidth { get; set; }

        protected override void PopulateDefaults()
        {
            //fire preset by default
            SpawnEnabled.DefaultValue = true;
            DespawnOutOfBounds.DefaultValue = true;
            SpawnTimeRange.DefaultValue = new FloatRange(0.0f, 0.2f);
            SpawnAmountRange.DefaultValue = new IntRange(5, 10);
            InitialYVelocityRange.DefaultValue = new FloatRange(-10, -5);
            InitialXVelocityRange.DefaultValue = new FloatRange(0, 0);
            MaxLifetimeRange.DefaultValue = new FloatRange(0, 2);
            Acceleration.DefaultValue = new SKPoint(0, 0);
            Drag.DefaultValue = new SKPoint(0, 0);
            SizeRange.DefaultValue = new FloatRange(20, 30);
            DeltaSize.DefaultValue = -20;
            SpawnPosition.DefaultValue = PropertyGroups.SpawnPosition.BottomEdge;
            Gradient.DefaultValue = new ColorGradient();
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Orange, 0));
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Red, 0.8f));
            Gradient.DefaultValue.Stops.Add(new ColorGradientStop(SKColors.Black, 1));

            DrawTrail.DefaultValue = false;
            TrailLength.DefaultValue = new SKPoint(0, 0);
            TrailWidth.DefaultValue = new SKPoint(0, 0);
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