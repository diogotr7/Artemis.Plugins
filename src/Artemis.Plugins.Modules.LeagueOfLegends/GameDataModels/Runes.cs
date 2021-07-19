namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class Runes
    {
        public Rune Keystone { get; set; } = new();
        public Rune PrimaryRuneTree { get; set; } = new();
        public Rune SecondaryRuneTree { get; set; } = new();
    }
}
