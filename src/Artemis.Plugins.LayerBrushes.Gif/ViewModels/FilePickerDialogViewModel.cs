using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.LayerBrushes.Gif.ViewModels
{
    public class FilePickerDialogViewModel : DialogViewModelBase
    {
        public void FilePicked(string file)
        {
            if (!Session.IsEnded)
                Session.Close(file);
        }

        public void Cancel()
        {
            Session.Close();
        }
    }
}