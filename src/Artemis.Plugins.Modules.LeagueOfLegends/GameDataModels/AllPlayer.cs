namespace Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels
{
    public class AllPlayer
    {
        public string ChampionName { get; set; }
        public bool IsBot { get; set; }
        public bool IsDead { get; set; }
        public Item[] Items { get; set; }
        public int Level { get; set; }
        public string Position { get; set; }
        public string RawChampionName { get; set; }
        public float RespawnTimer { get; set; }
        public Runes Runes { get; set; }
        public Scores Scores { get; set; }
        public int SkinID { get; set; }
        public string SummonerName { get; set; }
        public SummonerSpells SummonerSpells { get; set; }
        public string Team { get; set; }
    }
}
