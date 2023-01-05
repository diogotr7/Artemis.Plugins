using Artemis.Core;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels;

public class FilePathPropertyDisplayViewModel : PropertyInputViewModel<string>
{
    private readonly IWindowService _windowService;

    public FilePathPropertyDisplayViewModel(LayerProperty<string> layerProperty,
        IProfileEditorService profileEditorService,
        IPropertyInputService propertyInputService,
        IWindowService windowService) : base(layerProperty, profileEditorService, propertyInputService)
    {
        _windowService = windowService;

        Browse = ReactiveCommand.CreateFromTask(ExecuteBrowse);
    }

    public ReactiveCommand<Unit, Unit> Browse { get; }

    private async Task ExecuteBrowse()
    {
        var dialog = _windowService.CreateOpenFileDialog()
            .WithTitle("Pick gif")
            .HavingFilter(f => f.WithExtension("gif"));

        var files = await dialog.ShowAsync();
        if (files?.Length == 1)
        {
            InputValue = files[0];
        }
    }
}
