using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ReactiveUI;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Spotify;

public class SpotifyConfigurationDialogViewModel : PluginConfigurationViewModel
{
    private readonly HttpClient _client = new();
    private readonly SpotifyAccount _account;
    private readonly ILogger _logger;

    private Bitmap? _profilePicture;
    private string? _username;
    private string _status = string.Empty;
    private string? _loginError;
    private bool _isLoggedIn;
    private bool _isLoginExpired;
    private bool _isWaitingForUser;

    public SpotifyConfigurationDialogViewModel(Plugin plugin, SpotifyAccount account, ILogger logger) : base(plugin)
    {
        _account = account;
        _logger = logger;

        Login = ReactiveCommand.CreateFromTask(ExecuteLogin);
        OpenLoginPage = ReactiveCommand.Create(_account.OpenLoginPage);
        CancelLogin = ReactiveCommand.Create(_account.CancelLogin);
        Logout = ReactiveCommand.Create(_account.Logout);

        UpdateState();

        this.WhenActivated(d =>
        {
            _account.StateChanged += AccountOnStateChanged;
            _account.LoginFailed += AccountOnLoginFailed;
            Disposable.Create(() =>
            {
                _account.StateChanged -= AccountOnStateChanged;
                _account.LoginFailed -= AccountOnLoginFailed;
                _account.CancelLogin();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> OpenLoginPage { get; }
    public ReactiveCommand<Unit, Unit> CancelLogin { get; }
    public ReactiveCommand<Unit, Unit> Logout { get; }

    public Bitmap? ProfilePicture
    {
        get => _profilePicture;
        set => RaiseAndSetIfChanged(ref _profilePicture, value);
    }

    public string? Username
    {
        get => _username;
        set => RaiseAndSetIfChanged(ref _username, value);
    }

    public string Status
    {
        get => _status;
        set => RaiseAndSetIfChanged(ref _status, value);
    }

    public string? LoginError
    {
        get => _loginError;
        set => RaiseAndSetIfChanged(ref _loginError, value);
    }

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    public bool IsLoginExpired
    {
        get => _isLoginExpired;
        set => RaiseAndSetIfChanged(ref _isLoginExpired, value);
    }

    public bool IsWaitingForUser
    {
        get => _isWaitingForUser;
        set => RaiseAndSetIfChanged(ref _isWaitingForUser, value);
    }

    private async Task ExecuteLogin()
    {
        LoginError = null;
        try
        {
            await _account.StartLogin();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to start the Spotify login");
            _account.CancelLogin();
            LoginError = $"Couldn't start the login: {e.Message}";
        }
    }

    private void AccountOnStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(UpdateState);
    }

    private void AccountOnLoginFailed(object? sender, Exception e)
    {
        Dispatcher.UIThread.Post(() => LoginError = $"Spotify didn't accept the login: {e.Message}");
    }

    private void UpdateState()
    {
        bool wasLoggedIn = IsLoggedIn;
        IsLoggedIn = _account.IsLoggedIn;
        IsWaitingForUser = _account.IsWaitingForLogin;
        IsLoginExpired = _account.IsLoginExpired && !IsWaitingForUser;

        if (IsWaitingForUser)
            Status = "Waiting for you to finish logging in, check your browser";
        else if (IsLoggedIn)
            Status = "Connected to Spotify";
        else if (_account.IsLoginExpired)
            Status = "Login expired";
        else
            Status = "Not connected";

        if (!IsLoggedIn)
        {
            Username = "Spotify";
            ProfilePicture = null;
        }
        else if (!wasLoggedIn)
        {
            UpdateProfile();
        }
    }

    private void UpdateProfile()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                PrivateUser? user = await _account.GetUserInfo();
                if (user is null)
                    return;
                Username = user.DisplayName;

                //skiaSharp crashes on linux, fix later TODO
                if (OperatingSystem.IsLinux() || user.Images.Count < 1)
                    return;

                HttpResponseMessage response = await _client.GetAsync(user.Images.Last().Url, HttpCompletionOption.ResponseContentRead);
                Stream imageStream = await response.Content.ReadAsStreamAsync();
                ProfilePicture = new Bitmap(imageStream);
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to get the Spotify profile");
            }
        });
    }
}
