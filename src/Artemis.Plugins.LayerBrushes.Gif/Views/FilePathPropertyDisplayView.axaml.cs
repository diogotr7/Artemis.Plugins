using Artemis.Plugins.LayerBrushes.Gif.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.LayerBrushes.Gif.Views;

public partial class FilePathPropertyDisplayView : ReactiveUserControl<FilePathPropertyDisplayViewModel>
{
    public FilePathPropertyDisplayView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
