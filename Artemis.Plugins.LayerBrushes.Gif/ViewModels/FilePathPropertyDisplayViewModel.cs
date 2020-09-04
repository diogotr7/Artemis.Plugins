using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            LayerProperty.BaseValue = await _dialogService.ShowDialog<FilePickerDialogViewModel>() as string;
            NotifyOfPropertyChange(nameof(LayerProperty));
        }
    }
}
