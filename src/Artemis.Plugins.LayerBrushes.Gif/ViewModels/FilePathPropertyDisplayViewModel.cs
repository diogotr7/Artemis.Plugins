using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using System.Threading.Tasks;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class FilePathPropertyDisplayViewModel : PropertyInputViewModel<string>
    {
        private readonly IWindowService _windowService;

        public FilePathPropertyDisplayViewModel(LayerProperty<string> layerProperty,
            IProfileEditorService profileEditorService,
            IPropertyInputService propertyInputService,
            IWindowService windowService) : base(layerProperty, profileEditorService, propertyInputService)
        {
            _windowService = windowService;
        }

        public async Task Browse()
        {
            if ((await _windowService.ShowDialogAsync(new FilePickerDialogViewModel())) is string fileName)
            {
                InputValue = fileName;
            }
        }
    }
}
