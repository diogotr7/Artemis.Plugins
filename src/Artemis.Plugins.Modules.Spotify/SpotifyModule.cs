using Artemis.Core;
using Artemis.Core.ColorScience;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Spotify.DataModels;
using Serilog;
using SkiaSharp;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Spotify;

[PluginFeature(Name = "Spotify", AlwaysEnabled = true)]
public class SpotifyModule : Module<SpotifyDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

        #region Constructor and readonly fields
        private readonly ILogger _logger;
        private readonly PluginSetting<PKCETokenResponse> _token;
        private readonly PluginSetting<Dictionary<string, ColorSwatch>> _cache;
        private readonly HttpClient _httpClient;

        public SpotifyModule(PluginSettings settings, ILogger logger)
        {
            _logger = logger;
            _token = settings.GetSetting<PKCETokenResponse>(Constants.SPOTIFY_AUTH_SETTING);
            _cache = settings.GetSetting<Dictionary<string, ColorSwatch>>("AlbumArtCache", new());
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(2)
            };
        }
        #endregion

    private SpotifyClient? _spotify;
    private string? _trackId;
    private string? _contextId;
    private string? _albumArtUrl;
    private TrackAudioAnalysis? _analysis;

    #region Plugin Methods
    public override void Enable()
    {
        try
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
            AddDefaultProfile(DefaultCategoryName.Applications, Plugin.ResolveRelativePath("Spotify.json"));
        }
        catch (Exception e)
        {
            _logger.Error("Failed spotify authentication, login in the settings dialog:" + e.ToString());
        }
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

        CurrentlyPlayingContext? playing;
        try
        {
            playing = await _spotify.Player.GetCurrentPlayback();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating playback");
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
            _logger.Error(e, "Error updating player info");
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
        if (_spotify is null)
            return;
        
        string trackId = track.Uri.Split(':').Last();
        if (trackId == _trackId)
            return;
        
        UpdateBasicTrackInfo(track);

        try
        {
            TrackAudioFeatures features = await _spotify.Tracks.GetAudioFeatures(trackId);
            UpdateTrackFeatures(features);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating track audio features");
        }

        try
        {
            _analysis = await _spotify.Tracks.GetAudioAnalysis(trackId);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error getting track audio analysis");
        }

        Image image = track.Album.Images.First();
        if (image.Url != _albumArtUrl)
        {
            await UpdateAlbumArtColors(image.Url);
            _albumArtUrl = image.Url;
        }

        _trackId = trackId;
    }

    private async Task UpdatePlayerInfo(CurrentlyPlayingContext playing)
    {
        if (_spotify is null)
            return;
        
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
        var uri = albumArtUrl.Split('/').Last();
        DataModel.Track.AlbumArtUrl = albumArtUrl;

        lock (_cache)
        {
            if (_cache.Value.TryGetValue(uri, out var swatch))
            {
                DataModel.Track.Colors = swatch;
                return;
            }
        }

        try
        {
            var newSwatch = await GetAlbumColorsFromUri(albumArtUrl);
            lock (_cache)
            {
                _cache.Value[uri] = newSwatch;
                DataModel.Track.Colors = newSwatch;
                _cache.Save();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to get album art colors");
        }
    }

    private async Task<ColorSwatch> GetAlbumColorsFromUri(string uri)
    {
        using Stream stream = await _httpClient.GetStreamAsync(uri);
        using SKBitmap skbm = SKBitmap.Decode(stream);
        SKColor[] skClrs = ColorQuantizer.Quantize(skbm.Pixels, 256);
        return ColorQuantizer.FindAllColorVariations(skClrs, true);
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

    internal async Task<PrivateUser?> GetUserInfo()
    {
        if (_spotify is null)
            return null;
        
        try
        {
            return await _spotify.UserProfile.Current();
        }
        catch
        {
            return null;
        }
    }
        #endregion
    }