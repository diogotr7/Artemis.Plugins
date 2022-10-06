using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups
{
    public class ChromaPropertyGroup : LayerPropertyGroup
    {
       [PropertyDescription(Description = "Colors unmapped LEDs with the default color")]
        public LayerProperty<bool> UseDefaultLed { get; set; }

        [PropertyDescription(Description = "Turns black LEDs transparent")]
        public LayerProperty<bool> TransparentBlack { get; set; }

        protected override void PopulateDefaults()
        {
            UseDefaultLed.DefaultValue = true;
            TransparentBlack.DefaultValue = false;
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}