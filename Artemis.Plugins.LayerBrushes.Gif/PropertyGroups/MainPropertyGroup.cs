using System.ComponentModel;
using Artemis.Core;
using Artemis.Plugins.LayerBrushes.Gif.ViewModels;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Gif.PropertyGroups
{
    public class MainPropertyGroup : LayerPropertyGroup
    {
        [PropertyDescription(DisableKeyframes = true)]
        public LayerProperty<string> FileName { get; set; }

        protected override void PopulateDefaults()
        {
            FileName.DefaultValue = "";
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}