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
        private readonly ColorThief _colorThief = new ColorThief();
        private readonly HttpClient _httpClient = new HttpClient();
        private SpotifyClient _spotify;
        private CurrentlyPlayingContext _playing;
        private string _trackId;
        private string _contextId;

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

            _spotify = new SpotifyClient(config);

            AddTimedUpdate(TimeSpan.FromSeconds(2), UpdateData);
        }

        public override void DisablePlugin()
        { }

        public override void Update(double deltaTime) { }

        private async void UpdateData(double deltaTime)
        {
            try
            {
                _playing = await _spotify.Player.GetCurrentPlayback();
                if (_playing is null)
                    return;

                DataModel.Player.Shuffle = _playing.ShuffleState;
                DataModel.Player.RepeatState = Enum.Parse<RepeatState>(_playing.RepeatState, true);
                DataModel.Player.Volume = _playing.Device.VolumePercent ?? -1;
                DataModel.Player.IsPlaying = _playing.IsPlaying;
                DataModel.Track.Progress = TimeSpan.FromMilliseconds(_playing.ProgressMs);

                if (_playing.Context != null && Enum.TryParse<ContextType>(_playing.Context.Type, true, out var t))
                {
                    DataModel.Player.ContextType = t;
                    var contextId = _playing.Context.Uri.Split(':').Last();
                    if (contextId != _contextId)
                    {
                        DataModel.Player.ContextName = t switch
                        {
                            ContextType.Artist => (await _spotify.Artists.Get(contextId)).Name,
                            ContextType.Album => (await _spotify.Albums.Get(contextId)).Name,
                            ContextType.Playlist => (await _spotify.Playlists.Get(contextId)).Name,
                            _ => ""
                        };

                        _contextId = contextId;
                    }
                }
                else
                {
                    DataModel.Player.ContextType = ContextType.None;
                }

                if (_playing.Item is FullTrack track)
                {
                    var trackId = track.Uri.Split(':').Last();
                    if (trackId != _trackId)
                    {
                        UpdateBasicTrackInfo(track);

                        var features = await _spotify.Tracks.GetAudioFeatures(trackId);
                        UpdateTrackFeatures(features);

                        var image = track.Album.Images.FirstOrDefault();
                        await UpdateAlbumArtColors(image.Url);

                        _trackId = trackId;
                    }
                }
            }
            catch
            {
                //ignore
            }
        }

        private void UpdateBasicTrackInfo(FullTrack track)
        {
            DataModel.Track.Title = track.Name;
            DataModel.Track.Album = track.Album.Name;
            DataModel.Track.Artist = string.Join(", ", track.Artists.Select(a => a.Name));
            DataModel.Track.Popularity = track.Popularity;
            DataModel.Track.Duration = TimeSpan.FromMilliseconds(track.DurationMs);
        }

        private void UpdateTrackFeatures(TrackAudioFeatures features)
        {
            DataModel.Track.Features.Acousticness = features.Acousticness;
            DataModel.Track.Features.Danceability = features.Danceability;
            DataModel.Track.Features.Energy = features.Energy;
            DataModel.Track.Features.Instrumentalness = features.Instrumentalness;
            DataModel.Track.Features.Liveness = features.Liveness;
            DataModel.Track.Features.Loudness = features.Loudness;
            DataModel.Track.Features.Speechiness = features.Speechiness;
            DataModel.Track.Features.Tempo = features.Tempo;
            DataModel.Track.Features.Valence = features.Valence;
            DataModel.Track.Features.Key = features.Key;
            DataModel.Track.Features.Mode = features.Mode;
            DataModel.Track.Features.TimeSignature = features.TimeSignature;
        }

        private async Task UpdateAlbumArtColors(string albumArtUrl)
        {
            using var response = await _httpClient.GetAsync(albumArtUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var bitmap = new Bitmap(stream);

            var palette = _colorThief.GetPalette(bitmap, 32, 100, true);
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
            Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true, CreateNoWindow = true });
        }
    }
}