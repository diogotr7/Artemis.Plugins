using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.Modules.LeagueOfLegends.ViewModels
{
    public class CustomViewModel : ModuleViewModel
    {
        private readonly IDialogService _dialogService;
        private string _testString;
        private int _testNumber;

        // Note how we can inject the IDialogService or any other service into our view model using dependency injection
        public CustomViewModel(LeagueOfLegendsModule module, string displayName, IDialogService dialogService) : base(module, displayName)
        {
            _dialogService = dialogService;
        }

        // Regular auto-properties will not update the UI.
        // Because of that you must use SetAndNotify in your ViewModels as shown below
        public string TestString
        {
            get => _testString;
            set => SetAndNotify(ref _testString, value);
        }

        public int TestNumber
        {
            get => _testNumber;
            set => SetAndNotify(ref _testNumber, value);
        }

        // These two methods will be called when we press the buttons in our view
        public void TestPopupAction()
        {
            _dialogService.ShowConfirmDialog("Test popup", $"Looks like you entered: {TestString}");
        }

        public void TestNumberAction()
        {
            TestNumber++;
        }
    }
}