using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Chroma.LayerBrushes.PropertyGroups;

#pragma warning disable CS8618
public class ChromaPropertyGroup : LayerPropertyGroup
{
    [PropertyDescription(Description = "Colors unmapped LEDs with the default color")]
    public BoolLayerProperty UseDefaultLed { get; set; }

    [PropertyDescription(Description = "Turns black LEDs transparent")]
    public BoolLayerProperty TransparentBlack { get; set; }

    [PropertyDescription(Description = "Enhances colors to be more vibrant")]
    public BoolLayerProperty OverwatchEnhanceColors { get; set; }

    protected override void PopulateDefaults()
    {
        UseDefaultLed.DefaultValue = true;
        TransparentBlack.DefaultValue = false;
        OverwatchEnhanceColors.DefaultValue = false;
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}