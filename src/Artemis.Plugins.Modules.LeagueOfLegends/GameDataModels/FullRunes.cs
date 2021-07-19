using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class FullRunes
    {
        public Rune[] GeneralRunes { get; set; } = Array.Empty<Rune>();
        public Rune Keystone { get; set; } = new();
        public Rune PrimaryRuneTree { get; set; } = new();
        public Rune SecondaryRuneTree { get; set; } = new();
        public StatRune[] StatRunes { get; set; } = Array.Empty<StatRune>();
    }
}
