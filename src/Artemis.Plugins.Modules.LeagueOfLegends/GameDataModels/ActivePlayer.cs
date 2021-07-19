namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class ActivePlayer
    {
        public Abilities Abilities { get; set; } = new();
        public ChampionStats ChampionStats { get; set; } = new();
        public float CurrentGold { get; set; }
        public FullRunes FullRunes { get; set; } = new();
        public int Level { get; set; }
        public string SummonerName { get; set; }
    }
}
