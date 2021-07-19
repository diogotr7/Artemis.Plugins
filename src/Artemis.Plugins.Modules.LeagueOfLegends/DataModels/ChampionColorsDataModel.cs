using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class ChampionColorsDataModel
    {
        public ChampionColorsDataModel()
        {
            Default = new SKColor(128, 128, 128);
            Vibrant = new SKColor(128, 128, 128);
            LightVibrant = new SKColor(128, 128, 128);
            DarkVibrant = new SKColor(128, 128, 128);
            Muted = new SKColor(128, 128, 128);
            LightMuted = new SKColor(128, 128, 128);
            DarkMuted = new SKColor(128, 128, 128);
        }
        public SKColor Default { get; set; }
        public SKColor Vibrant { get; set; }
        public SKColor LightVibrant { get; set; }
        public SKColor DarkVibrant { get; set; }
        public SKColor Muted { get; set; }
        public SKColor LightMuted { get; set; }
        public SKColor DarkMuted { get; set; }
    }
}
