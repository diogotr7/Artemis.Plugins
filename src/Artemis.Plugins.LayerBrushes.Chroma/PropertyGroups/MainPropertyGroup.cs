using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Chroma.PropertyGroups
{
    public class MainPropertyGroup : LayerPropertyGroup
    {
       [PropertyDescription(Description = "Colors unmapped LEDs with the default color")]
        public LayerProperty<bool> UseDefaultLed { get; set; }

        protected override void PopulateDefaults()
        {
            UseDefaultLed.DefaultValue = true;
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}