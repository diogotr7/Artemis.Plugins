using Artemis.Core;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Controls;
using Avalonia.Threading;
using Serilog;
using System;

namespace Artemis.Plugins.Modules.Spotify;

public class SpotifyLoginNotifier : IDisposable
{
    private readonly Plugin _plugin;
    private readonly SpotifyAccount _account;
    private readonly INotificationService _notificationService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IWindowService _windowService;
    private readonly ILogger _logger;
    private bool _notified;
    private Window? _settingsWindow;

    public SpotifyLoginNotifier(Plugin plugin,
        SpotifyAccount account,
        INotificationService notificationService,
        IMainWindowService mainWindowService,
        IWindowService windowService,
        ILogger logger)
    {
        _plugin = plugin;
        _account = account;
        _notificationService = notificationService;
        _mainWindowService = mainWindowService;
        _windowService = windowService;
        _logger = logger;

        _account.LoginExpired += AccountOnLoginExpired;
        _account.StateChanged += AccountOnStateChanged;
    }

    public void Dispose()
    {
        _account.LoginExpired -= AccountOnLoginExpired;
        _account.StateChanged -= AccountOnStateChanged;
        _mainWindowService.MainWindowOpened -= MainWindowServiceOnMainWindowOpened;
    }

    private void AccountOnLoginExpired(object? sender, EventArgs e)
    {
        if (_notified)
            return;
        _notified = true;

        Dispatcher.UIThread.Post(() =>
        {
            // In-app notifications need an open window, wait for one if Artemis is in the tray
            if (_mainWindowService.IsMainWindowOpen)
                ShowNotification();
            else
                _mainWindowService.MainWindowOpened += MainWindowServiceOnMainWindowOpened;
        });
    }

    private void AccountOnStateChanged(object? sender, EventArgs e)
    {
        // A new login gets its own notification when it expires
        if (_account.IsLoggedIn)
            _notified = false;
    }

    private void MainWindowServiceOnMainWindowOpened(object? sender, EventArgs e)
    {
        _mainWindowService.MainWindowOpened -= MainWindowServiceOnMainWindowOpened;
        if (_account.IsLoginExpired)
            ShowNotification();
    }

    private void ShowNotification()
    {
        try
        {
            _notificationService.CreateNotification()
                .WithTitle("Spotify login expired")
                .WithMessage("Artemis stopped reading what's playing on Spotify. Log in again in the Spotify plugin settings.")
                .WithSeverity(NotificationSeverity.Warning)
                .WithTimeout(TimeSpan.FromSeconds(30))
                .HavingButton(b => b.WithText("Log in again").WithAction(OpenSettings))
                .Show();
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to show the Spotify login expired notification");
        }
    }

    private void OpenSettings()
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        try
        {
            SpotifyConfigurationDialogViewModel viewModel = _plugin.Resolve<SpotifyConfigurationDialogViewModel>();
            _settingsWindow = _windowService.ShowWindow(new SpotifySettingsWindowViewModel(viewModel));
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to open the Spotify settings");
        }
    }
}
