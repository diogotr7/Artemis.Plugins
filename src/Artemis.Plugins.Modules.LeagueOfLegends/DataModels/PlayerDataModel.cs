using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.Utils;
using SkiaSharp;
using System;
using SummonerSpell = Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums.SummonerSpell;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerDataModel : DataModel
    {
        public AbilityGroupDataModel Abilities { get; } = new();
        public PlayerStatsDataModel ChampionStats { get; } = new();
        public InventoryDataModel Inventory { get; } = new();
        public ColorSwatch ChampionColors { get; set; } = new();
        public SKColor DefaultChampionColor { get; set; }
        public string SummonerName { get; set; }
        public int Level { get; set; }
        public float Gold { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int CreepScore { get; set; }
        public float WardScore { get; set; }
        public float RespawnTimer { get; set; }
        public bool IsDead { get; set; }
        public Team Team { get; set; }
        public Champion Champion { get; set; }
        public string ChampionName { get; set; }
        public string InternalChampionName { get; set; }
        public int SkinID { get; set; }
        public Position Position { get; set; }
        public SummonerSpell SpellD { get; set; }
        public SummonerSpell SpellF { get; set; }

        public void Apply(RootGameData rootGameData)
        {
            var allPlayer = Array.Find(rootGameData.AllPlayers, p => p.SummonerName == rootGameData.ActivePlayer.SummonerName);
            if (allPlayer == null)
                return;

            Abilities.Apply(rootGameData.ActivePlayer.Abilities);
            ChampionStats.Apply(rootGameData.ActivePlayer.ChampionStats);
            Inventory.Apply(allPlayer.Items);

            SummonerName = rootGameData.ActivePlayer.SummonerName;
            Level = rootGameData.ActivePlayer.Level;
            Gold = rootGameData.ActivePlayer.CurrentGold;

            Kills = allPlayer.Scores.Kills;
            Deaths = allPlayer.Scores.Deaths;
            Assists = allPlayer.Scores.Assists;
            CreepScore = allPlayer.Scores.CreepScore;
            WardScore = allPlayer.Scores.WardScore;
            RespawnTimer = allPlayer.RespawnTimer;
            IsDead = allPlayer.IsDead;
            Team = ParseEnum<Team>.TryParseOr(allPlayer.Team, Team.Unknown);
            Champion = ParseEnum<Champion>.TryParseOr(allPlayer.ChampionName, Champion.Unknown);
            ChampionName = allPlayer.ChampionName;
            InternalChampionName = allPlayer.RawChampionName.Replace("game_character_displayname_", "");
            SkinID = allPlayer.SkinID;
            Position = ParseEnum<Position>.TryParseOr(allPlayer.Position, Position.Unknown);
            //TODO: change these to use the ID instead
            SpellD = ParseEnum<SummonerSpell>.TryParseOr(allPlayer.SummonerSpells.SummonerSpellOne.DisplayName, SummonerSpell.Unknown);
            SpellF = ParseEnum<SummonerSpell>.TryParseOr(allPlayer.SummonerSpells.SummonerSpellTwo.DisplayName, SummonerSpell.Unknown);
        }
    }
}
