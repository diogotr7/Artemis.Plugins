using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.Spotify.DataModels;
using ColorThiefDotNet;
using SkiaSharp;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        #region Auth
        private const string clientId = "78a0fd8e919e4ff58c4ddd7979bf00bf";
        private PluginSetting<PKCETokenResponse> token;
        private static EmbedIOAuthServer _server;
        private static EmbedIOAuthServer Server => _server ??= new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);
        #endregion

        private readonly PluginSettings _settings;
        private SpotifyClient spotify;
        private CurrentlyPlayingContext playing;
        private readonly ColorThief _colorThief = new ColorThief();
        private readonly HttpClient _httpClient = new HttpClient();
        private string albumArtUrl;

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
            }

            var authenticator = new PKCEAuthenticator(clientId, token.Value);
            authenticator.TokenRefreshed += (_, t) =>
            {
                token.Value = t;
                token.Save();
            };

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(authenticator);

            spotify = new SpotifyClient(config);

            AddTimedUpdate(TimeSpan.FromSeconds(2), UpdateData);
        }

        public override void DisablePlugin()
        { }

        public override void Update(double deltaTime)
        { }

        private async void UpdateData(double deltaTime)
        {
            try
            {
                playing = await spotify.Player.GetCurrentPlayback();
                if (playing is null)
                    return;

                DataModel.Player.Shuffle = playing.ShuffleState;
                DataModel.Player.VolumePercent = playing.Device.VolumePercent ?? -1;
                DataModel.Player.IsPlaying = playing.IsPlaying;

                if (!(playing.Item is FullTrack track))
                    return;

                if (!playing.IsPlaying)
                {
                    DataModel.Track.Reset();
                    return;
                }

                DataModel.Track.Title = track.Name;
                DataModel.Track.Album = track.Album.Name;
                DataModel.Track.Artist = string.Join(", ", track.Artists.Select(a => a.Name));
                DataModel.Track.Popularity = track.Popularity;

                var image = track.Album.Images.FirstOrDefault();
                if (image != null && image.Url != albumArtUrl)
                {
                    albumArtUrl = image.Url;
                    UpdateAlbumArtColors(albumArtUrl);
                }
            }
            catch
            {
                //ignore
            }
        }

        private async void UpdateAlbumArtColors(string albumArtUrl)
        {
            using var response = await _httpClient.GetAsync(albumArtUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var bitmap = new Bitmap(stream);

            var palette = _colorThief.GetPalette(bitmap, 10, 100, true);
            DataModel.Track.Colors = palette
                .Select(qc => new SKColor(qc.Color.R, qc.Color.G, qc.Color.B))
                .ToList();

            DataModel.Track.Color1 = DataModel.Track.Colors[0];
            DataModel.Track.Color2 = DataModel.Track.Colors[1];
        }

        private async Task StartAuth()
        {
            var (verifier, challenge) = PKCEUtil.GenerateCodes();

            await Server.Start();
            Server.AuthorizationCodeReceived += async (_, response) =>
            {
                await Server.Stop();

                token.Value = await new OAuthClient().RequestToken(
                  new PKCETokenRequest(clientId, response.Code, _server.BaseUri, verifier)
                );

                token.Save();
            };

            var request = new LoginRequest(_server.BaseUri, clientId, LoginRequest.ResponseType.Code)
            {
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
                Scope = new List<string> { Scopes.UserReadCurrentlyPlaying, Scopes.UserReadPlaybackState }
            };
            var url = request.ToUri();
            Process.Start(url.ToString());
        }
    }
}