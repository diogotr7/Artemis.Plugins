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

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        #region DI
        private readonly ILogger _logger;
        private readonly IColorQuantizerService _colorQuantizer;
        private readonly PluginSetting<PKCETokenResponse> _token;

        public SpotifyDataModelExpansion(PluginSettings settings, ILogger logger, IColorQuantizerService colorQuantizer)
        {
            _logger = logger;
            _colorQuantizer = colorQuantizer;
            _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
        }
        #endregion

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ConcurrentDictionary<string, TrackColorsDataModel> albumArtColorCache
                    = new ConcurrentDictionary<string, TrackColorsDataModel>();
        private SpotifyClient _spotify;
        private string _trackId;
        private string _contextId;
        private string _albumArtUrl;

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

            AddTimedUpdate(TimeSpan.FromSeconds(2), UpdateData);
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
            if (DataModel.Player.IsPlaying)
                DataModel.Track.Progress += TimeSpan.FromSeconds(deltaTime);
        }
        #endregion

        #region DataModel update methods
        private async Task UpdateData(double deltaTime)
        {
            //this will be null before authentication
            if (_spotify is null)
                return;
            
            try
            {
                CurrentlyPlayingContext playing = await _spotify.Player.GetCurrentPlayback();
                if (playing is null || DataModel is null)
                    return;

                await UpdatePlayerInfo(playing);

                //in theory this can also be FullEpisode for podcasts
                //but it does not seem to work correctly.
                if (playing.Item is FullTrack track)
                {
                    await UpdateTrackInfo(track);
                }
            }
            catch (APIException e)
            {
                _logger.Error(e.ToString());
            }
        }

        private async Task UpdateTrackInfo(FullTrack track)
        {
            string trackId = track.Uri.Split(':').Last();
            if (trackId != _trackId)
            {
                UpdateBasicTrackInfo(track);

                TrackAudioFeatures features = await _spotify.Tracks.GetAudioFeatures(trackId);
                UpdateTrackFeatures(features);

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
                        _ => ""
                    };

                    _contextId = contextId;
                }
            }
            else
            {
                DataModel.Player.ContextType = ContextType.None;
                DataModel.Player.ContextName = "";
                _contextId = "";
            }
        }

        private async Task UpdateAlbumArtColors(string albumArtUrl)
        {
            if (!albumArtColorCache.ContainsKey(albumArtUrl))
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(albumArtUrl);
                using Stream stream = await response.Content.ReadAsStreamAsync();
                using SKBitmap skbm = SKBitmap.Decode(stream);
                List<SKColor> skClrs = _colorQuantizer.Quantize(skbm.Pixels.ToList(), 256).ToList();
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