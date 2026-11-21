using Artemis.Core;
using Serilog;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Spotify;

/// <summary>
///     Represents the user's Spotify login. Events are raised on whatever thread caused the change, not the UI thread.
/// </summary>
public class SpotifyAccount : IDisposable
{
    // Must match a redirect URI configured on the Spotify app, Spotify doesn't accept localhost
    private static readonly Uri CallbackUri = new("http://127.0.0.1:5000/callback");

    private readonly PluginSetting<PKCETokenResponse> _token;
    private readonly ILogger _logger;
    private EmbedIOAuthServer? _server;
    private string? _verifier;

    public SpotifyAccount(PluginSettings settings, ILogger logger)
    {
        _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
        _logger = logger;

        try
        {
            if (_token.Value is not null)
                CreateClient();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed spotify authentication, login in the settings dialog");
        }
    }

    public SpotifyClient? Client { get; private set; }

    public bool IsLoggedIn => Client is not null;

    public bool IsLoginExpired { get; private set; }

    public string? LoginUrl { get; private set; }

    public bool IsWaitingForLogin => LoginUrl is not null;

    public event EventHandler? StateChanged;

    public event EventHandler? LoginExpired;

    public event EventHandler<Exception>? LoginFailed;

    public static bool IsInvalidGrant(Exception e)
    {
        // SpotifyAPI.Web uses the "error" field of the token endpoint's response as the message
        return e is APIException {Message: "invalid_grant"};
    }

    public async Task StartLogin()
    {
        StopServer();

        // Every attempt gets its own server, disposing the previous one frees the callback port
        _server = new EmbedIOAuthServer(CallbackUri, CallbackUri.Port);
        _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        await _server.Start();

        (_verifier, string challenge) = PKCEUtil.GenerateCodes();
        LoginRequest request = new(_server.BaseUri, Constants.SPOTIFY_CLIENT_ID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> {Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState}
        };
        LoginUrl = request.ToUri().ToString();
        OpenLoginPage();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OpenLoginPage()
    {
        if (LoginUrl is not null)
            Utilities.OpenUrl(LoginUrl);
    }

    public void CancelLogin()
    {
        if (!IsWaitingForLogin)
            return;

        StopServer();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Logout()
    {
        _token.Value = null;
        _token.Save();
        Client = null;
        IsLoginExpired = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Expire(Exception e)
    {
        if (Client is null)
            return;

        Client = null;
        IsLoginExpired = true;
        _logger.Warning(e, "Spotify rejected the saved login, log in again in the plugin settings");
        LoginExpired?.Invoke(this, EventArgs.Empty);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<PrivateUser?> GetUserInfo()
    {
        if (Client is null)
            return null;

        try
        {
            return await Client.UserProfile.Current();
        }
        catch (Exception e)
        {
            if (IsInvalidGrant(e))
                Expire(e);
            return null;
        }
    }

    public void Dispose()
    {
        StopServer();
    }

    private void CreateClient()
    {
        PKCEAuthenticator authenticator = new(Constants.SPOTIFY_CLIENT_ID, _token.Value!);
        authenticator.TokenRefreshed += (_, t) =>
        {
            _token.Value = t;
            _token.Save();
            _logger.Information("Refreshed spotify token!");
        };

        Client = new SpotifyClient(SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator));
        IsLoginExpired = false;
    }

    private void StopServer()
    {
        if (_server is not null)
        {
            _server.AuthorizationCodeReceived -= OnAuthorizationCodeReceived;
            _server.Dispose();
            _server = null;
        }

        _verifier = null;
        LoginUrl = null;
    }

    private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
    {
        // Stop rather than dispose, the server is still answering the browser, the next login disposes it
        EmbedIOAuthServer server = (EmbedIOAuthServer) sender;
        server.AuthorizationCodeReceived -= OnAuthorizationCodeReceived;
        await server.Stop();

        string? verifier = _verifier;
        _verifier = null;
        LoginUrl = null;

        if (verifier is not null)
        {
            try
            {
                PKCETokenRequest tokenRequest = new(Constants.SPOTIFY_CLIENT_ID, response.Code, server.BaseUri, verifier);
                _token.Value = await new OAuthClient().RequestToken(tokenRequest);
                _token.Save();
                CreateClient();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to finish the Spotify login");
                LoginFailed?.Invoke(this, e);
            }
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
