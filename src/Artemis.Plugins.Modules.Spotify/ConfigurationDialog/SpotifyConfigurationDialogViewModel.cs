﻿using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Spotify;

public class SpotifyConfigurationDialogViewModel : PluginConfigurationViewModel
{
    private readonly HttpClient _client = new();
    private readonly PluginSetting<PKCETokenResponse> _token;
    private readonly SpotifyModule _dataModelExpansion;

    private static EmbedIOAuthServer? _server;
    private static EmbedIOAuthServer Server => _server ??= new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

    private Bitmap? _profilePicture;

    public Bitmap? ProfilePicture
    {
        get => _profilePicture;
        set => RaiseAndSetIfChanged(ref _profilePicture, value);
    }

    private string? _username;

    public string? Username
    {
        get => _username;
        set => RaiseAndSetIfChanged(ref _username, value);
    }

    private bool _logInVisibility;

    public bool LogInVisibility
    {
        get => _logInVisibility;
        set => RaiseAndSetIfChanged(ref _logInVisibility, value);
    }

    private bool _logOutVisibility;

    public bool LogOutVisibility
    {
        get => _logOutVisibility;
        set => RaiseAndSetIfChanged(ref _logOutVisibility, value);
    }

    private string? _verifier;
    private string? _challenge;
    private string? _loginUrl;
    private bool _waitingForUser;

    public SpotifyConfigurationDialogViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
    {
        _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
        _dataModelExpansion = Plugin.GetFeature<SpotifyModule>()!;
        UpdateProfilePicture();
        UpdateButtonVisibility();
    }

    public async Task Login()
    {
        if (_waitingForUser)
            return;
        
        _waitingForUser = true;
        (_verifier, _challenge) = PKCEUtil.GenerateCodes();

        await Server.Start();
        Server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

        var request = new LoginRequest(Server.BaseUri, Constants.SPOTIFY_CLIENT_ID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = _challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
        };
        _loginUrl = request.ToUri().ToString();

        if (_loginUrl is not null)
            Utilities.OpenUrl(_loginUrl);
    }

    public void Logout()
    {
        _token.Value = null;
        _token.Save();
        _dataModelExpansion.Logout();
        UpdateProfilePicture();
        UpdateButtonVisibility();
    }

    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        Server.AuthorizationCodeReceived -= OnAuthorizationCodeReceived;
        await Server.Stop();

        var tokenRequest = new PKCETokenRequest(Constants.SPOTIFY_CLIENT_ID, response.Code, Server.BaseUri, _verifier!);

        _token.Value = await new OAuthClient().RequestToken(tokenRequest);
        _token.Save();

        _dataModelExpansion.Login();
        UpdateProfilePicture();
        UpdateButtonVisibility();
        _waitingForUser = false;
        _loginUrl = null;
    }

    private void UpdateButtonVisibility()
    {
        LogInVisibility = !_dataModelExpansion.LoggedIn;
        LogOutVisibility = _dataModelExpansion.LoggedIn;
    }

    private void UpdateProfilePicture()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                if (_dataModelExpansion.LoggedIn)
                {
                    var user = await _dataModelExpansion.GetUserInfo();
                    if (user is null)
                        return;
                    Username = user.DisplayName;

                    //skiaSharp crashes on linux, fix later TODO
                    if (OperatingSystem.IsLinux())
                        return;

                    if (user.Images.Count < 1)
                        return;
                    
                    try
                    {
                        var response = await _client.GetAsync(user.Images.Last().Url, HttpCompletionOption.ResponseContentRead);
                        var imageStream = await response.Content.ReadAsStreamAsync();
                        ProfilePicture = new Bitmap(imageStream);
                    }
                    catch (Exception e)
                    {
                        ProfilePicture = new Bitmap(new FileStream(Plugin.ResolveRelativePath("no-user.png"), FileMode.Open, FileAccess.Read));
                    }

                    return;
                }

                Username = "Not logged in";
            }
            catch (Exception e)
            {
                //failed to get user info, ignore
            }
        });
    }
}