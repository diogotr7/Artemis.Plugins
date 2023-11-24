using Artemis.Core.ColorScience;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using SpotifyAPI.Web;
using System;
using System.ComponentModel;

namespace Artemis.Plugins.Modules.Spotify.DataModels;

#pragma warning disable CS8618

public class SpotifyDataModel : DataModel
{
    public SpotifyPlayerDataModel Player { get; set; } = new SpotifyPlayerDataModel();
    public SpotifyTrackDataModel Track { get; set; } = new SpotifyTrackDataModel();
    public SpotifyDeviceDataModel Device { get; set; } = new SpotifyDeviceDataModel();
}

public class SpotifyPlayerDataModel : DataModel
{
    public bool IsPlaying { get; set; }
    public bool Shuffle { get; set; }
    public int Volume { get; set; }
    public RepeatState RepeatState { get; set; }
    public ContextType ContextType { get; set; }
    public string? ContextName { get; set; }
}

public class SpotifyTrackDataModel : DataModel
{
    public string Title { get; set; }
    public string Album { get; set; }
    public string Artist { get; set; }
    public int Popularity { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan Progress { get; set; }
    public ColorSwatch Colors { get; set; }
    public SpotifyTrackFeaturesDataModel Features { get; set; } = new SpotifyTrackFeaturesDataModel();
    public SpotifyTrackAnalysisDataModel Analysis { get; set; } = new SpotifyTrackAnalysisDataModel();
    public string AlbumArtUrl { get; set; }
}

public class SpotifyTrackFeaturesDataModel : DataModel
{
    [DataModelProperty(Affix = "%", Description = "Level of confidence the track is acoustic")]
    public float Acousticness { get; set; }

    [DataModelProperty(Affix = "%", Description = "How suitable a track is for dancing")]
    public float Danceability { get; set; }

    [DataModelProperty(Affix = "%", Description = "How energetic / intense a track feels")]
    public float Energy { get; set; }

    [DataModelProperty(Affix = "%", Description = "How likely a track is to contain no spoken words")]
    public float Instrumentalness { get; set; }

    [DataModelProperty(Affix = "%", Description = "How likely a track is to have a live audience")]
    public float Liveness { get; set; }

    [DataModelProperty(Affix = "dB", Description = "Overall perceived loudness of a track")]
    public float Loudness { get; set; }

    [DataModelProperty(Affix = "%", Description = "How likely a track is to contain spoken words")]
    public float Speechiness { get; set; }

    [DataModelProperty(Affix = "%", Description = "How musically positive the track is (happy, cheerful)")]
    public float Valence { get; set; }

    [DataModelProperty(Affix = "BMP", Description = "Overall estimated tempo of a track in beats per minute")]
    public float Tempo { get; set; }

    [DataModelProperty(Description = "Estimated overall key of the track.")]
    public Key Key { get; set; }

    public Mode Mode { get; set; }

    public int TimeSignature { get; set; }
}

public enum Key
{
    None = -1,
    C = 0,
    [Description("C#")] Cs = 1,
    D = 2,
    [Description("D#")] Ds = 3,
    E = 4,
    F = 5,
    [Description("F#")] Fs = 6,
    G = 7,
    [Description("G#")] Gs = 8,
    A = 9,
    [Description("A#")] As = 10,
    B = 11,
}

public enum Mode
{
    Minor = 0,
    Major = 1
}

public enum ContextType
{
    None,
    Playlist,
    Album,
    Artist
}

public enum RepeatState
{
    Off,
    Context,
    Track
}

public class SpotifyTrackAnalysisDataModel : DataModel
{
    public Section CurrentSection { get; internal set; }
    public Segment CurrentSegment { get; internal set; }
}

public class SpotifyDeviceDataModel : DataModel
{
    public string Name { get; set; }
    public string Type { get; set; }
}