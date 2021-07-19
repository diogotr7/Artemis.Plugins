namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class FullRunes
    {
        public Rune[] GeneralRunes { get; set; }
        public Rune Keystone { get; set; }
        public Rune PrimaryRuneTree { get; set; }
        public Rune SecondaryRuneTree { get; set; }
        public StatRune[] StatRunes { get; set; }
    }
}
