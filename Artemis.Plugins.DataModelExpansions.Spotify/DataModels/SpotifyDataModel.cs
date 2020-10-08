using Artemis.Core.DataModelExpansions;
using SkiaSharp;
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
        public int VolumePercent { get; set; }
    }

    public class SpotifyTrackDataModel : DataModel
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public int Popularity { get; set; }
        public SKColor Color1 { get; set; }
        public SKColor Color2 { get; set; }
        public List<SKColor> Colors { get; set; }

        internal void Reset()
        {
            Title = "";
            Album = "";
            Artist = "";
            Popularity = -1;
            Color1 = new SKColor();
            Color2 = new SKColor();
            Colors = new List<SKColor>();
        }
    }
}