using Artemis.Core.DataModelExpansions;
using SkiaSharp;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Spotify.DataModels
{
    public class SpotifyDataModel : DataModel
    {
        public SpotifyPlayerDataModel Player { get; set; } = new SpotifyPlayerDataModel();
        public SpotifyTrackDataModel Track { get; set; } = new SpotifyTrackDataModel();
    }

    public class SpotifyPlayerDataModel : DataModel
    {
        public bool IsPlaying { get; set; }
        public bool Shuffle { get; set; }
        public int Volume { get; set; }
        public RepeatState RepeatState { get; set; }
        public ContextType ContextType { get; set; }
        public string ContextName { get; set; }
    }

    public class SpotifyTrackDataModel : DataModel
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public int Popularity { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Progress { get; set; }
        public SKColor Color1 { get; set; }
        public SKColor Color2 { get; set; }
        public List<SKColor> Colors { get; set; }
        public SpotifyTrackFeaturesDataModel Features { get; set; } = new SpotifyTrackFeaturesDataModel();
    }

    public class SpotifyTrackFeaturesDataModel : DataModel
    {
        public float Acousticness { get; set; }
        public float Danceability { get; set; }
        public float Energy { get; set; }
        public float Instrumentalness { get; set; }
        public float Liveness { get; set; }
        public float Loudness { get; set; }
        public float Speechiness { get; set; }
        public float Tempo { get; set; }
        public float Valence { get; set; }
        public int Key { get; set; }
        public int Mode { get; set; }
        public int TimeSignature { get; set; }
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
}