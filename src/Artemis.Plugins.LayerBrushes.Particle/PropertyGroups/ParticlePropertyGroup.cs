using Artemis.Core;
using Artemis.Core.DefaultTypes;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticlePropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public BoolLayerProperty SpawnEnabled { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public SKPointLayerProperty SpawnTime { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public SKPointLayerProperty SpawnAmount { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty InitialYVelocity { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty InitialXVelocity { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public SKPointLayerProperty MaxLifetime { get; set; }

        [PropertyDescription]
        public SKPointLayerProperty Acceleration { get; set; }

        [PropertyDescription(MinInputValue = 0, MaxInputValue = 1)]
        public SKPointLayerProperty Drag { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public SKPointLayerProperty Size { get; set; }

        [PropertyDescription]
        public FloatLayerProperty DeltaSize { get; set; }

        [PropertyDescription]
        public EnumLayerProperty<SpawnPosition> SpawnPosition { get; set; }

        [PropertyDescription]
        public ColorGradientLayerProperty Gradient { get; set; }

        protected override void PopulateDefaults()
        {
            SpawnEnabled.DefaultValue = true;
            SpawnTime.DefaultValue = new SkiaSharp.SKPoint(0.5f, 1f);
            SpawnAmount.DefaultValue = new SkiaSharp.SKPoint(1, 10);
            InitialYVelocity.DefaultValue = new SkiaSharp.SKPoint(0, 0);
            InitialXVelocity.DefaultValue = new SkiaSharp.SKPoint(0, 0);
            MaxLifetime.DefaultValue = new SkiaSharp.SKPoint(0, 5);
            Acceleration.DefaultValue = new SkiaSharp.SKPoint(0, -1);
            Drag.DefaultValue = new SkiaSharp.SKPoint(0, 0);
            Size.DefaultValue = new SkiaSharp.SKPoint(6, 12);
            DeltaSize.DefaultValue = 0;
            SpawnPosition.DefaultValue = PropertyGroups.SpawnPosition.Top;
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
        Top,
        Right,
        Bottom,
        Left,
        Random
    }

    public enum ColorMode
    {
        LifetimeBased,
        Static
    }
}