using Artemis.Core;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Particle.PropertyGroups;

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
        TrailLength.DefaultValue = new SKPoint(0f, 0f);
        TrailWidth.DefaultValue = new SKPoint(0f, 0f);
    }

    protected override void EnableProperties() { }

    protected override void DisableProperties() { }
}
