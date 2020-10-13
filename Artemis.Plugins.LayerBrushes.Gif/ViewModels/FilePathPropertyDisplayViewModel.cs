using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class FilePathPropertyDisplayViewModel : PropertyInputViewModel<string>
    {
        private readonly IDialogService _dialogService;
        public FilePathPropertyDisplayViewModel(LayerProperty<string> layerProperty, IProfileEditorService profileEditorService, IDialogService dialogService) : base(layerProperty, profileEditorService)
        {
            _dialogService = dialogService;
        }

        public async void Browse()
        {
            if (await _dialogService.ShowDialog<FilePickerDialogViewModel>() is string fileName)
            {
                LayerProperty.BaseValue = fileName;
                NotifyOfPropertyChange(nameof(LayerProperty));
            }
        }
    }
}
