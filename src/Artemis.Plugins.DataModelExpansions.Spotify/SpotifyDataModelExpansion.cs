using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using Artemis.Plugins.DataModelExpansions.Spotify.DataModels;
using Serilog;
using SkiaSharp;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        #region Constructor and readonly fields
        private readonly ILogger _logger;
        private readonly IColorQuantizerService _colorQuantizer;
        private readonly PluginSetting<PKCETokenResponse> _token;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, TrackColorsDataModel> albumArtColorCache;

        public SpotifyDataModelExpansion(PluginSettings settings, ILogger logger, IColorQuantizerService colorQuantizer)
        {
            _logger = logger;
            _colorQuantizer = colorQuantizer;
            _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
            albumArtColorCache = new ConcurrentDictionary<string, TrackColorsDataModel>();
        }
        #endregion

        private SpotifyClient _spotify;
        private string _trackId;
        private string _contextId;
        private string _albumArtUrl;
        private TrackAudioAnalysis _analysis;

        #region Plugin Methods
        public override void Enable()
        {
            try
            {
                Login();
            }
            catch (Exception e)
            {
                _logger.Error("Failed spotify authentication, login in the settings dialog:" + e.ToString());
            }

            AddTimedUpdate(TimeSpan.FromSeconds(2), UpdatePlayback, nameof(UpdatePlayback));
        }

        public override void Disable()
        {
            _spotify = null;
            _trackId = null;
            _contextId = null;
            _albumArtUrl = null;
        }

        public override void Update(double deltaTime)
        {
            if (!DataModel.Player.IsPlaying)
                return;

            DataModel.Track.Progress += TimeSpan.FromSeconds(deltaTime);

            if (_analysis is null)
                return;

            var currentSeconds = DataModel.Track.Progress.TotalSeconds;
            var currentSection = _analysis.Sections.Find(s => currentSeconds > s.Start &&
                                                              currentSeconds < s.Start + s.Duration);
            if (currentSection != null)
            {
                DataModel.Track.Analysis.CurrentSection = currentSection;
            }

            var currentSegment = _analysis.Segments.Find(s => currentSeconds > s.Start &&
                                                              currentSeconds < s.Start + s.Duration);
            if (currentSegment != null)
            {
                DataModel.Track.Analysis.CurrentSegment = currentSegment;
            }
        }
        #endregion

        #region DataModel update methods
        private async Task UpdatePlayback(double deltaTime)
        {
            //this will be null before authentication
            if (_spotify is null)
                return;

            CurrentlyPlayingContext playing;
            try
            {
                playing = await _spotify.Player.GetCurrentPlayback();
            }
            catch (Exception e)
            {
                _logger.Error("Error updating playback", e);
                return;
            }

            if (playing is null || DataModel is null)
                return;//weird

            try
            {
                await UpdatePlayerInfo(playing);
            }
            catch (Exception e)
            {
                _logger.Error("Error updating player info", e);
            }

            //in theory this can also be FullEpisode for podcasts
            //but it does not seem to work correctly.
            if (playing.Item is FullTrack track)
            {
                await UpdateTrackInfo(track);
            }
        }

        private async Task UpdateTrackInfo(FullTrack track)
        {
            string trackId = track.Uri.Split(':').Last();
            if (trackId != _trackId)
            {
                UpdateBasicTrackInfo(track);

                try
                {
                    TrackAudioFeatures features = await _spotify.Tracks.GetAudioFeatures(trackId);
                    UpdateTrackFeatures(features);
                }
                catch (Exception e)
                {
                    _logger.Error("Error updating track audio features", e);
                }

                try
                {
                    _analysis = await _spotify.Tracks.GetAudioAnalysis(trackId);
                }
                catch (Exception e)
                {
                    _logger.Error("Error getting track audio analysis", e);
                }

                Image image = track.Album.Images.Last();
                if (image.Url != _albumArtUrl)
                {
                    await UpdateAlbumArtColors(image.Url);
                    _albumArtUrl = image.Url;
                }

                _trackId = trackId;
            }
        }

        private async Task UpdatePlayerInfo(CurrentlyPlayingContext playing)
        {
            DataModel.Device.Name = playing.Device.Name;
            DataModel.Device.Type = playing.Device.Type;
            DataModel.Player.Shuffle = playing.ShuffleState;
            DataModel.Player.RepeatState = Enum.Parse<RepeatState>(playing.RepeatState, true);
            DataModel.Player.Volume = playing.Device.VolumePercent ?? -1;
            DataModel.Player.IsPlaying = playing.IsPlaying;
            DataModel.Track.Progress = TimeSpan.FromMilliseconds(playing.ProgressMs);

            if (playing.Context != null && Enum.TryParse(playing.Context.Type, true, out ContextType contextType))
            {
                DataModel.Player.ContextType = contextType;
                string contextId = playing.Context.Uri.Split(':').Last();
                if (contextId != _contextId)
                {
                    DataModel.Player.ContextName = contextType switch
                    {
                        ContextType.Artist => (await _spotify.Artists.Get(contextId)).Name,
                        ContextType.Album => (await _spotify.Albums.Get(contextId)).Name,
                        ContextType.Playlist => (await _spotify.Playlists.Get(contextId)).Name,
                        _ => string.Empty
                    };

                    _contextId = contextId;
                }
            }
            else
            {
                DataModel.Player.ContextType = ContextType.None;
                DataModel.Player.ContextName = string.Empty;
                _contextId = string.Empty;
            }
        }

        private async Task UpdateAlbumArtColors(string albumArtUrl)
        {
            if (!albumArtColorCache.ContainsKey(albumArtUrl))
            {
                try
                {
                    using HttpResponseMessage response = await _httpClient.GetAsync(albumArtUrl);
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using SKBitmap skbm = SKBitmap.Decode(stream);
                    SKColor[] skClrs = _colorQuantizer.Quantize(skbm.Pixels, 256);
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
                catch (Exception e)
                {
                    _logger.Error("Failed to get album art colors", e);
                }
            }

            if (albumArtColorCache.TryGetValue(albumArtUrl, out var colorDataModel))
                DataModel.Track.Colors = colorDataModel;
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

        #endregion

        #region VM interaction
        internal bool LoggedIn => _spotify != null;

        internal void Login()
        {
            PKCEAuthenticator authenticator = new PKCEAuthenticator(Constants.SPOTIFY_CLIENT_ID, _token.Value);
            authenticator.TokenRefreshed += (_, t) =>
            {
                _token.Value = t;
                _token.Save();
                _logger.Information("Refreshed spotify token!");
            };

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);

            _spotify = new SpotifyClient(config);
        }

        internal void Logout()
        {
            _spotify = null;
        }

        internal async Task<PrivateUser> GetUserInfo()
        {
            if (_spotify is null)
                return null;

            return await _spotify.UserProfile.Current();
        }
        #endregion
    }
}