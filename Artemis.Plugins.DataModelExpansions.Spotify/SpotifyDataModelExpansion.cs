using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.Spotify.DataModels;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        private readonly PluginSettings _settings;
        private PluginSetting<PKCETokenResponse> token;
        private const string clientId = "78a0fd8e919e4ff58c4ddd7979bf00bf";

        private static EmbedIOAuthServer _server;
        private static EmbedIOAuthServer Server => _server ??= new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
        private SpotifyClient spotify;
        private CurrentlyPlayingContext playing;
        private Timer updateTimer;

        public SpotifyDataModelExpansion(PluginSettings settings)
        {
            _settings = settings;
        }

        public override void EnablePlugin()
        {
            token = _settings.GetSetting<PKCETokenResponse>("Authorization", null);
            if (token.Value is null)
            {
                Task.Run(StartAuth);
                while (token.Value == null)
                {
                    Thread.Sleep(1000);
                }
                //spinlock here?
            }

            var authenticator = new PKCEAuthenticator(clientId, token.Value);
            authenticator.TokenRefreshed += (_, t) =>
            {
                token.Value = t;
                token.Save();
            };

            var config = SpotifyClientConfig.CreateDefault()
              .WithAuthenticator(authenticator);

            spotify = new SpotifyClient(config);

            updateTimer = new Timer(1000);
            updateTimer.Elapsed += UpdateData;
            updateTimer.Start();
        }

        private async Task StartAuth()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await Server.Start();
            Server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await Server.Stop();

                token.Value = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(clientId!, response.Code, _server.BaseUri, verifier)
                );

                token.Save();
            };

            var request = new LoginRequest(_server.BaseUri, clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
            };

            Process.Start(new ProcessStartInfo("cmd", $"/c start {request.ToUri()}")
            {
                CreateNoWindow = true
            });
        }

        public override void DisablePlugin()
        {
            updateTimer?.Dispose();
        }

        public override void Update(double deltaTime)
        {
            if (playing is null)
                return;

            if (playing.Item is FullTrack track)
            {
                DataModel.Track.Title = track.Name;
                DataModel.Track.Artist = string.Join(", ", track.Artists.Select(a => a.Name));
                DataModel.Track.Album = track.Album.Name;
                DataModel.Track.Info = track.Album.Images.FirstOrDefault()?.Url ?? "";
                DataModel.Player.ProgressMs = (float)playing.ProgressMs / track.DurationMs;
            }
            DataModel.Player.IsPlaying = playing.IsPlaying;
            DataModel.Player.Shuffle = playing.ShuffleState;
        }

        private async void UpdateData(object sender, ElapsedEventArgs e)
        {
            playing = await spotify.Player.GetCurrentPlayback();
        }
    }
}