using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using System;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerDataModel : DataModel
    {
        public AbilityGroupDataModel Abilities { get; set; } = new AbilityGroupDataModel();
        public PlayerStatsDataModel ChampionStats { get; set; } = new PlayerStatsDataModel();
        public InventoryDataModel Inventory { get; set; } = new InventoryDataModel();
        public string SummonerName { get; set; } = "";
        public int Level { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CreepScore { get; set; }
        public float Gold { get; set; }
        public float WardScore { get; set; }
        public float RespawnTimer { get; set; }
        public bool IsDead { get; set; }
        public Team Team { get; set; }
        public Champion Champion { get; set; }
        public Position Position { get; set; }
        public SummonerSpell SpellD { get; set; }
        public SummonerSpell SpellF { get; set; }

        internal void Reset()
        {
            Abilities.Reset();
            ChampionStats.Reset();
            Inventory.Reset();
            SummonerName = "";
            Level = -1;
            Kills = -1;
            Deaths = -1;
            Assists = -1;
            CreepScore = -1;
            Gold = -1;
            WardScore = -1;
            RespawnTimer = -1;
            IsDead = false;
            Team = Team.None;
            Champion = Champion.None;
            Position = Position.None;
            SpellD = SummonerSpell.None;
            SpellF = SummonerSpell.None;
        }
    }
}
