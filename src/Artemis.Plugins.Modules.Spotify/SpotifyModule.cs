using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Spotify.DataModels;
using Serilog;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Services;

namespace Artemis.Plugins.Modules.Spotify;

[PluginFeature(Name = "Spotify", AlwaysEnabled = true)]
public class SpotifyModule : Module<SpotifyDataModel>
{
    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

    private readonly SpotifyAccount _account;
    private readonly AlbumArtColors _albumArtColors;
    private readonly ILogger _logger;

    private string? _trackId;
    private string? _contextId;
    private string? _albumArtUrl;
    private TrackAudioAnalysis? _analysis;

    public SpotifyModule(SpotifyAccount account, AlbumArtColors albumArtColors, ILogger logger)
    {
        _account = account;
        _albumArtColors = albumArtColors;
        _logger = logger;
    }

    public override void Enable()
    {
        AddTimedUpdate(TimeSpan.FromSeconds(2), UpdatePlayback, nameof(UpdatePlayback));
        AddDefaultProfile(DefaultCategoryName.Applications, Plugin.ResolveRelativePath("Spotify.zip"));
    }

    public override void Disable()
    {
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

    private async Task UpdatePlayback(double deltaTime)
    {
        //this will be null before authentication
        SpotifyClient? spotify = _account.Client;
        if (spotify is null)
            return;

        CurrentlyPlayingContext? playing;
        try
        {
            playing = await spotify.Player.GetCurrentPlayback();
        }
        catch (Exception e) when (SpotifyAccount.IsInvalidGrant(e))
        {
            _account.Expire(e);
            DataModel.Player.IsPlaying = false;
            return;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating playback");
            return;
        }

        if (playing is null || DataModel is null)
            return; //weird

        try
        {
            await UpdatePlayerInfo(spotify, playing);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating player info");
        }

        //in theory this can also be FullEpisode for podcasts
        //but it does not seem to work correctly.
        if (playing.Item is FullTrack track)
        {
            await UpdateTrackInfo(spotify, track);
        }
    }

    private async Task UpdateTrackInfo(SpotifyClient spotify, FullTrack track)
    {
        if (track.Id == _trackId)
            return;

        UpdateBasicTrackInfo(track);

        try
        {
            TrackAudioFeatures features = await spotify.Tracks.GetAudioFeatures(track.Id);
            UpdateTrackFeatures(features);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error updating track audio features");
        }

        try
        {
            _analysis = await spotify.Tracks.GetAudioAnalysis(track.Id);
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

        _trackId = track.Id;
    }

    private async Task UpdatePlayerInfo(SpotifyClient spotify, CurrentlyPlayingContext playing)
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
                    ContextType.Artist => (await spotify.Artists.Get(contextId)).Name,
                    ContextType.Album => (await spotify.Albums.Get(contextId)).Name,
                    ContextType.Playlist => (await spotify.Playlists.Get(contextId)).Name,
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
        DataModel.Track.AlbumArtUrl = albumArtUrl;

        try
        {
            DataModel.Track.Colors = await _albumArtColors.Get(albumArtUrl);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to get album art colors");
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
}
