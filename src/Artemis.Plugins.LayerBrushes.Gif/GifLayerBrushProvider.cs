using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Gif.ViewModels;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.LayerBrushes.Gif
{
    public class GifLayerBrushProvider : LayerBrushProvider
    {
        private readonly IProfileEditorService _profileEditorService;

        public GifLayerBrushProvider(IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
        }

        public override void Enable()
        {
            _profileEditorService.RegisterPropertyInput<FilePathPropertyDisplayViewModel>(Plugin);
            RegisterLayerBrushDescriptor<GifLayerBrush>("Gif layer brush", "Gif layer brush", "Gif");
        }

        public override void Disable()
        {
        }
    }
}