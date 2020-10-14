using Artemis.Core;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyConfigurationDialogViewModel : PluginConfigurationViewModel
    {
        private static EmbedIOAuthServer _server;
        private static EmbedIOAuthServer Server => _server ??= new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

        private readonly PluginSettings _settings;
        private readonly PluginSetting<PKCETokenResponse> _token;
        private string _verifier;
        private string _challenge;

        public SpotifyConfigurationDialogViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
        {
            _settings = settings;
            _token = _settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
        }

        public async void StartAuthentication()
        {
            (_verifier, _challenge) = PKCEUtil.GenerateCodes();

            await Server.Start();
            Server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

            var request = new LoginRequest(_server.BaseUri, Constants.SPOTIFY_CLIENT_ID, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = _challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
            };

            Utilities.OpenUrl(request.ToUri().ToString());
        }

        private async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            Server.AuthorizationCodeReceived -= OnAuthorizationCodeReceived;
            await Server.Stop();

            var tokenRequest = new PKCETokenRequest(Constants.SPOTIFY_CLIENT_ID, response.Code, _server.BaseUri, _verifier);

            _token.Value = await new OAuthClient().RequestToken(tokenRequest);
            _token.Save();

            (Plugin as SpotifyDataModelExpansion)?.InitializeSpotifyClient();
        }
    }
}