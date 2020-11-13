using Artemis.Core;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.UI.Shared;
using SkiaSharp;
using Stylet;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog
{
    public class LeagueOfLegendsConfigurationDialogViewModel : PluginConfigurationViewModel
    {
        private readonly PluginSetting<Dictionary<Champion, SKColor>> _colors;

        public BindableCollection<ChampionColor> Colors { get; set; }

        public LeagueOfLegendsConfigurationDialogViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
        {
            _colors = settings.GetSetting("LeagueOfLegendsChampionColors", DefaultChampionColors.Colors);
            Colors = new BindableCollection<ChampionColor>(_colors.Value.Select(kvp => new ChampionColor(kvp.Key, Convert(kvp.Value))));
        }

        private Color Convert(SKColor sk) => Color.FromArgb(sk.Alpha, sk.Red, sk.Green, sk.Blue);

        private SKColor Convert(Color clr) => new SKColor(clr.R, clr.G, clr.B, clr.A);

        public void SaveChanges()
        {
            _colors.Value = Colors.ToDictionary(kvp => kvp.Champion, kvp => Convert(kvp.Color));
            _colors.Save();

            RequestClose();
        }

        public void Cancel()
        {
            RequestClose();
        }

        public void Reset()
        {
            Colors = new BindableCollection<ChampionColor>(DefaultChampionColors.Colors.Select(kvp => new ChampionColor(kvp.Key, Convert(kvp.Value))));
            NotifyOfPropertyChange(nameof(Colors));
        }
    }

    public class ChampionColor
    {
        public Champion Champion { get; set; }
        public Color Color { get; set; }

        public ChampionColor(Champion c, Color clr)
        {
            Champion = c;
            Color = clr;
        }
    }
}
