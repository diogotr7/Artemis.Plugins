using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class FilePickerDialogViewModel : DialogViewModelBase<string>
    {
        public void FilePicked(string file)
        {
            //TODO
            //if (!Session.IsEnded)
            //    Session.Close(file);
        }
    }
}