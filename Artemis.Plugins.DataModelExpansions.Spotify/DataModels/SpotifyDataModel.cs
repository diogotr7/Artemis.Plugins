using System.Collections.Generic;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.Spotify.DataModels
{
    public class SpotifyDataModel : DataModel
    {
        public SpotifyTrackDataModel Track { get; set; } = new SpotifyTrackDataModel();

        public SpotifyDeviceDataModel Device { get; set; } = new SpotifyDeviceDataModel();

        public SpotifyPlayerDataModel Player { get; set; } = new SpotifyPlayerDataModel();
    }

    public class SpotifyPlayerDataModel : DataModel
    {
        public bool IsPlaying { get; internal set; }
        public bool Shuffle { get; internal set; }
        public float ProgressMs { get; internal set; }
    }

    public class SpotifyDeviceDataModel : DataModel
    {
    }

    public class SpotifyTrackDataModel : DataModel
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Info { get; set; }
    }
}