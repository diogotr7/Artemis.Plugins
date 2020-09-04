using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class FilePathPropertyInputViewModel : PropertyInputViewModel<string>
    {
        public FilePathPropertyInputViewModel(LayerProperty<string> layerProperty, IProfileEditorService profileEditorService, IDialogService dialogService) : base(layerProperty, profileEditorService)
        {

        }
    }

    public class DummyString
    {
        public string ActualString { get; set; }
    }
}
