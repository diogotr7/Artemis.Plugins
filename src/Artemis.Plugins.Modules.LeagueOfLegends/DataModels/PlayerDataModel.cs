using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using SkiaSharp;
using System;
using System.Collections.Generic;
using SummonerSpell = Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums.SummonerSpell;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerDataModel : ChildDataModel
    {
        private readonly Func<AllPlayer> allPlayer;
        internal Dictionary<Champion, SKColor> colorDictionary;

        public PlayerDataModel(LeagueOfLegendsDataModel root) : base(root)
        {
            allPlayer = () => RootGameData.AllPlayers == null
                ? default
                : Array.Find(RootGameData.AllPlayers, p => p.SummonerName == RootGameData.ActivePlayer.SummonerName);
            Abilities = new AbilityGroupDataModel(root);
            ChampionStats = new PlayerStatsDataModel(root);
            Inventory = new InventoryDataModel(allPlayer);
            ChampionColors = new ChampionColorsDataModel();
        }

        public AbilityGroupDataModel Abilities { get; set; }
        public PlayerStatsDataModel ChampionStats { get; set; }
        public InventoryDataModel Inventory { get; set; }
        public ChampionColorsDataModel ChampionColors { get; set; }
        public string SummonerName => RootGameData.ActivePlayer.SummonerName;
        public int Level => RootGameData.ActivePlayer.Level;
        public float Gold => RootGameData.ActivePlayer.CurrentGold;
        public int Kills => allPlayer().Scores.Kills;
        public int Deaths => allPlayer().Scores.Deaths;
        public int Assists => allPlayer().Scores.Assists;
        public int CreepScore => allPlayer().Scores.CreepScore;
        public float WardScore => allPlayer().Scores.WardScore;
        public float RespawnTimer => allPlayer().RespawnTimer;
        public bool IsDead => allPlayer().IsDead;
        public Team Team => ParseEnum<Team>.TryParseOr(allPlayer().Team, Team.Unknown);
        public string Champion => allPlayer().ChampionName;
        public string InternalChampionName => allPlayer().RawChampionName.Replace("game_character_displayname_", "");
        public int SkinID => allPlayer().SkinID;
        public Position Position => ParseEnum<Position>.TryParseOr(allPlayer().Position, Position.Unknown);
        public SummonerSpell SpellD => ParseEnum<SummonerSpell>.TryParseOr(allPlayer().SummonerSpells.SummonerSpellOne.DisplayName, SummonerSpell.Unknown);
        public SummonerSpell SpellF => ParseEnum<SummonerSpell>.TryParseOr(allPlayer().SummonerSpells.SummonerSpellTwo.DisplayName, SummonerSpell.Unknown);
    }
}
