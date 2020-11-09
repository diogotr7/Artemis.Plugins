using Artemis.Core;
using Artemis.Core.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups
{
    public class ParticleSpawnPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public BoolLayerProperty SpawnEnabled { get; set; }

        [PropertyDescription]
        public BoolLayerProperty DespawnOutOfBounds { get; set; }

        [PropertyDescription]
        public EnumLayerProperty<SpawnPosition> SpawnPosition { get; set; }

        [PropertyDescription(MinInputValue = 0, MaxInputValue = 1)]
        public SKPointLayerProperty SpawnPositionPercent { get; set; }

        [PropertyDescription(MinInputValue = 0, MaxInputValue = 100)]
        public IntRangeLayerProperty SpawnAmountRange { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public FloatRangeLayerProperty SpawnTimeRange { get; set; }

        [PropertyDescription]
        public FloatRangeLayerProperty InitialXVelocityRange { get; set; }

        [PropertyDescription]
        public FloatRangeLayerProperty InitialYVelocityRange { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public FloatRangeLayerProperty MaxLifetimeRange { get; set; }

        [PropertyDescription(MinInputValue = 0)]
        public FloatRangeLayerProperty SizeRange { get; set; }

        protected override void PopulateDefaults()
        {
            SpawnEnabled.DefaultValue = true;
            DespawnOutOfBounds.DefaultValue = true;
            SpawnPosition.DefaultValue = PropertyGroups.SpawnPosition.BottomEdge;
            SpawnPositionPercent.DefaultValue = new SKPoint(0.5f, 0.5f);
            SpawnAmountRange.DefaultValue = new IntRange(5, 10);
            SpawnTimeRange.DefaultValue = new FloatRange(0.0f, 0.2f);
            InitialXVelocityRange.DefaultValue = new FloatRange(0, 0);
            InitialYVelocityRange.DefaultValue = new FloatRange(-10, -5);
            MaxLifetimeRange.DefaultValue = new FloatRange(0, 2);
            SizeRange.DefaultValue = new FloatRange(20, 30);
        }

        protected override void EnableProperties()
        {
            SpawnPosition.CurrentValueSet += SpawnPositionOnCurrentValueSet;
            UpdateVisibility();
        }

        protected override void DisableProperties()
        {
            SpawnPosition.CurrentValueSet -= SpawnPositionOnCurrentValueSet;
        }

        private void SpawnPositionOnCurrentValueSet(object sender, LayerPropertyEventArgs<SpawnPosition> e)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            SpawnPositionPercent.IsHidden = SpawnPosition.BaseValue != PropertyGroups.SpawnPosition.Custom;
        }
    }
}
