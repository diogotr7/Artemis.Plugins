namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class ActivePlayer
    {
        public Abilities Abilities { get; set; }
        public ChampionStats ChampionStats { get; set; }
        public float CurrentGold { get; set; }
        public FullRunes FullRunes { get; set; }
        public int Level { get; set; }
        public string SummonerName { get; set; }
    }
}
