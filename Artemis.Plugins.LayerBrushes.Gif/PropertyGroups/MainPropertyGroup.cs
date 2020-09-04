using System.ComponentModel;
using Artemis.Core;

namespace Artemis.Plugins.LayerBrushes.Gif.PropertyGroups
{
    public class MainPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription]
        public LayerProperty<string> FileName { get; set; }

        protected override void PopulateDefaults()
        {
            FileName.DefaultValue = "";
        }

        protected override void EnableProperties()
        {
            // This is where you do any sort of initialization on your property group
        }

        protected override void DisableProperties()
        {
            // If you subscribed to events or need to clean up, do it here
        }
    }
}