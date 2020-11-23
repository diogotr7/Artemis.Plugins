using Artemis.Core;
using Artemis.UI.Shared;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using SkiaSharp.Views.WPF;
using System.Net.Http;
using System.IO;
using SkiaSharp;
using System.Windows;
using System.Windows.Threading;
using Stylet;
using System.Windows.Media.Imaging;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyConfigurationDialogViewModel : PluginConfigurationViewModel
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly PluginSetting<PKCETokenResponse> _token;
        private readonly SpotifyDataModelExpansion _dataModelExpansion;

        private static EmbedIOAuthServer _server;
        private static EmbedIOAuthServer Server => _server ??= new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

        private ImageSource _profilePicture;
        public ImageSource ProfilePicture
        {
            get => _profilePicture;
            set => SetAndNotify(ref _profilePicture, value);
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetAndNotify(ref _username, value);
        }

        private bool _logInVisibility;
        public bool LogInVisibility
        {
            get => _logInVisibility;
            set => SetAndNotify(ref _logInVisibility, value);
        }

        private bool _logOutVisibility;
        public bool LogOutVisibility
        {
            get => _logOutVisibility;
            set => SetAndNotify(ref _logOutVisibility, value);
        }

        private string _verifier;
        private string _challenge;
        private string _loginUrl;
        private bool _waitingForUser;

        public SpotifyConfigurationDialogViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
        {
            _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
            _dataModelExpansion = Plugin.GetFeature<SpotifyDataModelExpansion>();
            UpdateProfilePicture();
            UpdateButtonVisibility();
        }

        public async Task Login()
        {
            if (!_waitingForUser)
            {
                _waitingForUser = true;
                (_verifier, _challenge) = PKCEUtil.GenerateCodes();

                await Server.Start();
                Server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

                LoginRequest request = new LoginRequest(_server.BaseUri, Constants.SPOTIFY_CLIENT_ID, LoginRequest.ResponseType.Code)
                {
                    CodeChallenge = _challenge,
                    CodeChallengeMethod = "S256",
                    Scope = new List<string> { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
                };
                _loginUrl = request.ToUri().ToString();
            }

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

            PKCETokenRequest tokenRequest = new PKCETokenRequest(Constants.SPOTIFY_CLIENT_ID, response.Code, _server.BaseUri, _verifier);

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
            Execute.PostToUIThread(async () =>
            {
                ProfilePicture = new DrawingImage();
                Username = "Not logged in";

                if (_dataModelExpansion.LoggedIn)
                {
                    var user = await _dataModelExpansion.GetUserInfo();
                    if (user is null)
                        return;
                    Username = user.DisplayName;
                    if (user.Images.Count < 1)
                        return;

                    using HttpResponseMessage response = await _httpClient.GetAsync(user.Images[0].Url);
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    var image = SKBitmap.Decode(stream);

                    ProfilePicture = image.ToWriteableBitmap();
                }
            });
        }
    }
}