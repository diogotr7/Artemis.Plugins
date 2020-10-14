using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.Spotify.DataModels;
using ColorThiefDotNet;
using SkiaSharp;
using SpotifyAPI.Web;
using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace Artemis.Plugins.DataModelExpansions.Spotify
{
    public class SpotifyDataModelExpansion : DataModelExpansion<SpotifyDataModel>
    {
        #region DI
        private readonly PluginSettings _settings;
        private readonly ILogger _logger;

        public SpotifyDataModelExpansion(PluginSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }
        #endregion

        private readonly ColorThief _colorThief = new ColorThief();
        private HttpClient _httpClient = new HttpClient();
        private SpotifyClient _spotify;
        private CurrentlyPlayingContext _playing;
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

            if (true)
            {
                #region SKColor method
                using var skbm = SKBitmap.Decode(stream);
                var skClrs = ColorQuantizer.GetQuantizedColors(skbm.Pixels.ToList(), 128).ToList();
                var fixedPop = 128;
                DataModel.Track.Colors = skClrs;
                DataModel.Track.Vibrant = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.Vibrant);
                DataModel.Track.LightVibrant = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.LightVibrant);
                DataModel.Track.DarkVibrant = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.DarkVibrant);
                DataModel.Track.Muted = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.Muted);
                DataModel.Track.LightMuted = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.LightMuted);
                DataModel.Track.DarkMuted = Vibrant.FindColorVariation(skClrs, fixedPop, ColorType.DarkMuted);
                #endregion
            }
            else
            {
                #region colorthief method
                using var bitmap = new Bitmap(stream);
                var clrs = _colorThief.GetPalette(bitmap, 128, 10, false);
                var maxPop = clrs.Max(qc => qc.Population);
                DataModel.Track.Colors = clrs.Select(qc => qc.ToSKColor()).ToList();
                DataModel.Track.Vibrant = Vibrant.FindColorVariation(clrs, maxPop, ColorType.Vibrant).ToSKColor();
                DataModel.Track.LightVibrant = Vibrant.FindColorVariation(clrs, maxPop, ColorType.LightVibrant).ToSKColor();
                DataModel.Track.DarkVibrant = Vibrant.FindColorVariation(clrs, maxPop, ColorType.DarkVibrant).ToSKColor();
                DataModel.Track.Muted = Vibrant.FindColorVariation(clrs, maxPop, ColorType.Muted).ToSKColor();
                DataModel.Track.LightMuted = Vibrant.FindColorVariation(clrs, maxPop, ColorType.LightMuted).ToSKColor();
                DataModel.Track.DarkMuted = Vibrant.FindColorVariation(clrs, maxPop, ColorType.DarkMuted).ToSKColor();
                #endregion
            }
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