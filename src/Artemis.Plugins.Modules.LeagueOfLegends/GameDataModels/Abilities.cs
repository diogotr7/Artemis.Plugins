namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class Abilities
    {
        public Passive Passive { get; set; } = new();
        public Ability Q { get; set; } = new();
        public Ability W { get; set; } = new();
        public Ability E { get; set; } = new();
        public Ability R { get; set; } = new();
    }
}
