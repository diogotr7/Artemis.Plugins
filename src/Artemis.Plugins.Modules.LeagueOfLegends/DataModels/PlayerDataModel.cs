using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using SummonerSpell = Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums.SummonerSpell;

namespace Artemis.Plugins.Modules.LeagueOfLegends.DataModels
{
    public class PlayerDataModel : ChildDataModel
    {
        private readonly Func<AllPlayer> allPlayer;
        internal ConcurrentDictionary<Champion, SKColor> colorDictionary;

        public PlayerDataModel(LeagueOfLegendsDataModel root) : base(root)
        {
            allPlayer = () => RootGameData.AllPlayers == null
                ? default
                : Array.Find(RootGameData.AllPlayers, p => p.SummonerName == RootGameData.ActivePlayer.SummonerName);
            Abilities = new AbilityGroupDataModel(root);
            ChampionStats = new PlayerStatsDataModel(root);
            Inventory = new InventoryDataModel(allPlayer);
        }

        public AbilityGroupDataModel Abilities { get; set; }
        public PlayerStatsDataModel ChampionStats { get; set; }
        public InventoryDataModel Inventory { get; set; }
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
        public Champion Champion => ParseEnum<Champion>.TryParseOr(allPlayer().ChampionName, Champion.Unknown);
        public Position Position => ParseEnum<Position>.TryParseOr(allPlayer().Position, Position.Unknown);
        public SummonerSpell SpellD => ParseEnum<SummonerSpell>.TryParseOr(allPlayer().SummonerSpells.SummonerSpellOne.DisplayName, SummonerSpell.Unknown);
        public SummonerSpell SpellF => ParseEnum<SummonerSpell>.TryParseOr(allPlayer().SummonerSpells.SummonerSpellTwo.DisplayName, SummonerSpell.Unknown);
        public SKColor ChampionColor => colorDictionary[Champion];
    }
}
