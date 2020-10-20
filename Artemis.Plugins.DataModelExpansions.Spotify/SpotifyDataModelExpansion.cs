using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using Artemis.Plugins.DataModelExpansions.Spotify.DataModels;
using Serilog;
using SkiaSharp;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        #region DI
        private readonly PluginSettings _settings;
        private readonly ILogger _logger;
        private readonly IColorQuantizerService _colorQuantizer;

        public SpotifyDataModelExpansion(PluginSettings settings, ILogger logger, IColorQuantizerService colorQuantizer)
        {
            _settings = settings;
            _logger = logger;
            _colorQuantizer = colorQuantizer;
        }
        #endregion

        private HttpClient _httpClient = new HttpClient();
        private SpotifyClient _spotify;
        private CurrentlyPlayingContext _playing;
        private Dictionary<string, TrackColorsDataModel> albumArtColorCache = new Dictionary<string, TrackColorsDataModel>();
        private string _trackId;
        private string _contextId;
        private string _albumArtUrl;

        #region Plugin Methods
        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<SpotifyConfigurationDialogViewModel>();
            _httpClient = new HttpClient();

            try
            {
                InitializeSpotifyClient();
            }
            catch (Exception e)
            {
                _logger.Error("Failed spotify authentication, login in the settings dialog:" + e.ToString());
            }

            AddTimedUpdate(TimeSpan.FromSeconds(2), UpdateData);
        }

        public override void DisablePlugin()
        {
            _httpClient.Dispose();
            _spotify = null;
            _playing = null;
            _trackId = null;
            _contextId = null;
        }

        //unused
        public override void Update(double deltaTime) { }
        #endregion

        #region DataModel update methods
        private async void UpdateData(double deltaTime)
        {
            //this will be null before authentication
            if (_spotify is null)
                return;

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

                        var image = track.Album.Images.Last();
                        if (image.Url != _albumArtUrl)
                        {
                            await UpdateAlbumArtColors(image.Url);
                            _albumArtUrl = image.Url;
                        }

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
            DataModel.Track.Features.Acousticness = features.Acousticness * 100;
            DataModel.Track.Features.Danceability = features.Danceability * 100;
            DataModel.Track.Features.Energy = features.Energy * 100;
            DataModel.Track.Features.Instrumentalness = features.Instrumentalness * 100;
            DataModel.Track.Features.Liveness = features.Liveness * 100;
            DataModel.Track.Features.Loudness = features.Loudness;
            DataModel.Track.Features.Speechiness = features.Speechiness * 100;
            DataModel.Track.Features.Valence = features.Valence * 100;
            DataModel.Track.Features.Tempo = features.Tempo;
            DataModel.Track.Features.Key = (Key)features.Key;
            DataModel.Track.Features.Mode = (Mode)features.Mode;
            DataModel.Track.Features.TimeSignature = features.TimeSignature;
        }

        private async Task UpdateAlbumArtColors(string albumArtUrl)
        {
            if (!albumArtColorCache.ContainsKey(albumArtUrl))
            {
                using var response = await _httpClient.GetAsync(albumArtUrl);
                using var stream = await response.Content.ReadAsStreamAsync();
                using var skbm = SKBitmap.Decode(stream);
                var skClrs = _colorQuantizer.Quantize(skbm.Pixels.ToList(), 256).ToList();
                albumArtColorCache[albumArtUrl] = new TrackColorsDataModel
                {
                    Vibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.Vibrant, true),
                    LightVibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.LightVibrant, true),
                    DarkVibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.DarkVibrant, true),
                    Muted = _colorQuantizer.FindColorVariation(skClrs, ColorType.Muted, true),
                    LightMuted = _colorQuantizer.FindColorVariation(skClrs, ColorType.LightMuted, true),
                    DarkMuted = _colorQuantizer.FindColorVariation(skClrs, ColorType.DarkMuted, true),
                };
            }

            DataModel.Track.Colors = albumArtColorCache[albumArtUrl];
        }
        #endregion

        #region Spotify Client 

        private PluginSetting<PKCETokenResponse> token;

        internal void InitializeSpotifyClient()
        {
            token = _settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);

            var authenticator = new PKCEAuthenticator(Constants.SPOTIFY_CLIENT_ID, token.Value);
            authenticator.TokenRefreshed += (_, t) =>
            {
                token.Value = t;
                token.Save();
                _logger.Information("Refreshed spotify token!");
            };

            var config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);

            _spotify = new SpotifyClient(config);
        }
        #endregion
    }
}