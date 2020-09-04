using Artemis.Core.LayerBrushes;
using Artemis.UI.Shared.Services;
using Artemis.Plugins.LayerBrushes.Gif.PropertyGroups;
using MaterialDesignThemes.Wpf;
using MaterialDesignExtensions.Controls;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class GifConfigurationViewModel : BrushConfigurationViewModel
    {
        public GifConfigurationViewModel(GifLayerBrush layerBrush) : base(layerBrush)
        {
            Properties = layerBrush.Properties;
        }

        public MainPropertyGroup Properties { get; }

        // These two methods will be called when we press the buttons in our view
        public void FilePicked(string test)
        {
            Properties.FileName.BaseValue = test;
            RequestClose();
        }
    }
}