using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.Plugins.Modules.Spotify;

public partial class SpotifySettingsWindowView : ReactiveAppWindow<SpotifySettingsWindowViewModel>
{
    public SpotifySettingsWindowView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            SpotifySettingsWindowViewModel vm = ViewModel!;
            Observable.FromEventPattern(x => vm.ConfigurationViewModel.CloseRequested += x, x => vm.ConfigurationViewModel.CloseRequested -= x)
                .Subscribe(_ => Close())
                .DisposeWith(d);
        });
    }
}
