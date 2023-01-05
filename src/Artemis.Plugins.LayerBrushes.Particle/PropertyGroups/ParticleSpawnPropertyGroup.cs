using Artemis.Core;
using Artemis.Plugins.LayerBrushes.Particle.PropertyGroups.Enums;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups;

#pragma warning disable CS8618

public class ParticleSpawnPropertyGroup : LayerPropertyGroup
{
    [PropertyDescription]
    public BoolLayerProperty SpawnEnabled { get; set; }

    [PropertyDescription]
    public BoolLayerProperty DespawnOutOfBounds { get; set; }

    [PropertyDescription]
    public EnumLayerProperty<SpawnPosition> SpawnPosition { get; set; }

    [PropertyDescription(MinInputValue = 0f, MaxInputValue = 1f)]
    public SKPointLayerProperty SpawnPositionPercent { get; set; }

    [PropertyDescription(MinInputValue = 0, MaxInputValue = 100)]
    public IntRangeLayerProperty SpawnAmountRange { get; set; }

    [PropertyDescription(MinInputValue = 0f)]
    public FloatRangeLayerProperty SpawnTimeRange { get; set; }

    [PropertyDescription]
    public FloatRangeLayerProperty InitialXVelocityRange { get; set; }

    [PropertyDescription]
    public FloatRangeLayerProperty InitialYVelocityRange { get; set; }

    [PropertyDescription(MinInputValue = 0f)]
    public FloatRangeLayerProperty MaxLifetimeRange { get; set; }

    [PropertyDescription(MinInputValue = 0f)]
    public FloatRangeLayerProperty SizeRange { get; set; }

    protected override void PopulateDefaults()
    {
        SpawnEnabled.DefaultValue = true;
        DespawnOutOfBounds.DefaultValue = true;
        SpawnPosition.DefaultValue = Enums.SpawnPosition.BottomEdge;
        SpawnPositionPercent.DefaultValue = new SKPoint(0.5f, 0.5f);
        SpawnAmountRange.DefaultValue = new IntRange(5, 10);
        SpawnTimeRange.DefaultValue = new FloatRange(0.0f, 0.2f);
        InitialXVelocityRange.DefaultValue = new FloatRange(0f, 0f);
        InitialYVelocityRange.DefaultValue = new FloatRange(-10f, -5f);
        MaxLifetimeRange.DefaultValue = new FloatRange(0f, 2f);
        SizeRange.DefaultValue = new FloatRange(20f, 30f);
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

    private void SpawnPositionOnCurrentValueSet(object? sender, LayerPropertyEventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        SpawnPositionPercent.IsHidden = SpawnPosition.BaseValue != Enums.SpawnPosition.Custom;
    }
}
