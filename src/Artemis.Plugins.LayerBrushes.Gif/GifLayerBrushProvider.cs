using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Gif.ViewModels;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.Plugins.LayerBrushes.Gif
{
    public class GifLayerBrushProvider : LayerBrushProvider
    {
        private readonly IPropertyInputService _propertyInputService;

        public GifLayerBrushProvider(IPropertyInputService profileEditorService)
        {
            _propertyInputService = profileEditorService;
        }

        public override void Enable()
        {
            _propertyInputService.RegisterPropertyInput<FilePathPropertyDisplayViewModel>(Plugin);
            RegisterLayerBrushDescriptor<GifLayerBrush>("Gif layer brush", "Gif layer brush", "Gif");
        }

        public override void Disable()
        {
        }
    }
}