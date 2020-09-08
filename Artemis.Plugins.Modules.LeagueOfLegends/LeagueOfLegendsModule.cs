using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Linq;
using System.Net.Http;
using System.Timers;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsModule : ProfileModule<LeagueOfLegendsDataModel>
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private _RootGameData allGameData;
        private Timer updateTimer;

        public override void EnablePlugin()
        {
            DisplayName = "League Of Legends";
            DisplayIconPath = "LeagueOfLegendsIcon.png";
            DefaultPriorityCategory = ModulePriorityCategory.Application;
            ActivationRequirements.Add(new ProcessActivationRequirement("League Of Legends"));

            httpClientHandler = new HttpClientHandler
            {
                //we need this to not make the user install Riot's certificate on their computer
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            httpClient = new HttpClient(httpClientHandler);

            updateTimer = new Timer(100);
            updateTimer.Elapsed += UpdateData;
        }

        public override void DisablePlugin()
        {
            httpClient?.CancelPendingRequests();
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
            updateTimer?.Dispose();
            allGameData = null;
        }

        public override void ModuleActivated(bool isOverride)
        {
            if (isOverride)
                return;

            updateTimer?.Start();
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            if (isOverride)
                return;

            updateTimer?.Stop();
            httpClient?.CancelPendingRequests();
        }

        public override void Update(double deltaTime)
        {
            var dm = DataModel;

            if (allGameData == null)
            {
                DataModel.Reset();
                return;
            }

            #region Match
            dm.Match.InGame = true;
            dm.Match.GameMode = TryParseOr(allGameData.gameData.gameMode, true, GameMode.Unknown);
            dm.Match.GameModeName = allGameData.gameData.gameMode;
            dm.Match.GameTime = allGameData.gameData.gameTime;

            var drags = allGameData.events.Events.OfType<_DragonKillEvent>();

            dm.Match.InfernalDragonsKilled = drags.Count(d => string.Equals(d.DragonType, "fire", StringComparison.OrdinalIgnoreCase));
            dm.Match.EarthDragonsKilled = drags.Count(d => string.Equals(d.DragonType, "earth", StringComparison.OrdinalIgnoreCase));
            dm.Match.OceanDragonsKilled = drags.Count(d => string.Equals(d.DragonType, "water", StringComparison.OrdinalIgnoreCase));
            dm.Match.CloudDragonsKilled = drags.Count(d => string.Equals(d.DragonType, "air", StringComparison.OrdinalIgnoreCase));
            dm.Match.ElderDragonsKilled = drags.Count(d => string.Equals(d.DragonType, "elder", StringComparison.OrdinalIgnoreCase));
            dm.Match.DragonsKilled = drags.Count();

            dm.Match.BaronsKilled = allGameData.events.Events.Count(ev => ev is _BaronKillEvent);
            dm.Match.HeraldsKilled = allGameData.events.Events.Count(ev => ev is _HeraldKillEvent);
            dm.Match.TurretsKilled = allGameData.events.Events.Count(ev => ev is _TurretKillEvent);
            dm.Match.InhibsKilled = allGameData.events.Events.Count(ev => ev is _InhibKillEvent);
            dm.Match.MapTerrain = TryParseOr(allGameData.gameData.mapTerrain, true, MapTerrain.Unknown);
            #endregion

            #region Player
            var ap = allGameData.activePlayer;
            dm.Player.Level = ap.level;
            dm.Player.Gold = ap.currentGold;
            dm.Player.SummonerName = ap.summonerName;

            dm.Player.Abilities.Q.Level = ap.abilities.Q.abilityLevel;
            dm.Player.Abilities.Q.Name = ap.abilities.Q.displayName;
            dm.Player.Abilities.W.Level = ap.abilities.W.abilityLevel;
            dm.Player.Abilities.W.Name = ap.abilities.W.displayName;
            dm.Player.Abilities.E.Level = ap.abilities.E.abilityLevel;
            dm.Player.Abilities.E.Name = ap.abilities.E.displayName;
            dm.Player.Abilities.R.Level = ap.abilities.R.abilityLevel;
            dm.Player.Abilities.R.Name = ap.abilities.R.displayName;

            dm.Player.ChampionStats.AbilityPower = ap.championStats.abilityPower;
            dm.Player.ChampionStats.Armor = ap.championStats.armor;
            dm.Player.ChampionStats.ArmorPenetrationFlat = ap.championStats.armorPenetrationFlat;
            dm.Player.ChampionStats.ArmorPenetrationPercent = ap.championStats.armorPenetrationPercent;
            dm.Player.ChampionStats.AttackDamage = ap.championStats.attackDamage;
            dm.Player.ChampionStats.AttackRange = ap.championStats.attackRange;
            dm.Player.ChampionStats.AttackSpeed = ap.championStats.attackSpeed;
            dm.Player.ChampionStats.BonusArmorPenetrationPercent = ap.championStats.bonusArmorPenetrationPercent;
            dm.Player.ChampionStats.BonusMagicPenetrationPercent = ap.championStats.bonusMagicPenetrationPercent;
            dm.Player.ChampionStats.CooldownReduction = ap.championStats.cooldownReduction;
            dm.Player.ChampionStats.CritChance = ap.championStats.critChance;
            dm.Player.ChampionStats.CritDamagePercent = ap.championStats.critDamage;
            dm.Player.ChampionStats.HealthCurrent = ap.championStats.currentHealth;
            dm.Player.ChampionStats.HealthRegenRate = ap.championStats.healthRegenRate;
            dm.Player.ChampionStats.LifeSteal = ap.championStats.lifeSteal;
            dm.Player.ChampionStats.MagicLethality = ap.championStats.magicLethality;
            dm.Player.ChampionStats.MagicPenetrationFlat = ap.championStats.magicPenetrationFlat;
            dm.Player.ChampionStats.MagicPenetrationPercent = ap.championStats.magicPenetrationPercent;
            dm.Player.ChampionStats.MagicResist = ap.championStats.magicResist;
            dm.Player.ChampionStats.HealthMax = ap.championStats.maxHealth;
            dm.Player.ChampionStats.MoveSpeed = ap.championStats.moveSpeed;
            dm.Player.ChampionStats.PhysicalLethality = ap.championStats.physicalLethality;
            dm.Player.ChampionStats.ResourceMax = ap.championStats.resourceMax;
            dm.Player.ChampionStats.ResourceRegenRate = ap.championStats.resourceRegenRate;
            dm.Player.ChampionStats.ResourceType = TryParseOr(ap.championStats.resourceType, true, ResourceType.Unknown);
            dm.Player.ChampionStats.ResourceCurrent = ap.championStats.resourceValue;
            dm.Player.ChampionStats.SpellVamp = ap.championStats.spellVamp;
            dm.Player.ChampionStats.Tenacity = ap.championStats.tenacity;

            var p = allGameData.allPlayers.FirstOrDefault(a => a.summonerName == ap.summonerName);
            if (p == null)
            {
                return;
            }

            dm.Player.Champion = TryParseOr(p.championName.Replace(" ", "").Replace("'", "").Replace(".", ""), true, Champion.Unknown);
            dm.Player.SpellD = TryParseOr(p.summonerSpells.summonerSpellOne.displayName, true, SummonerSpell.Unknown);
            dm.Player.SpellF = TryParseOr(p.summonerSpells.summonerSpellTwo.displayName, true, SummonerSpell.Unknown);
            dm.Player.Team = TryParseOr(p.team, true, Team.Unknown);
            dm.Player.Position = TryParseOr(p.position, true, Position.Unknown);

            dm.Player.IsDead = p.isDead;
            dm.Player.RespawnTimer = p.respawnTimer;
            dm.Player.Kills = p.scores.kills;
            dm.Player.Deaths = p.scores.deaths;
            dm.Player.Assists = p.scores.assists;
            dm.Player.CreepScore = p.scores.creepScore;
            dm.Player.WardScore = p.scores.wardScore;

            dm.Player.Inventory.Slot1 = GetItem(p, 0);
            dm.Player.Inventory.Slot2 = GetItem(p, 1);
            dm.Player.Inventory.Slot3 = GetItem(p, 2);
            dm.Player.Inventory.Slot4 = GetItem(p, 3);
            dm.Player.Inventory.Slot5 = GetItem(p, 4);
            dm.Player.Inventory.Slot6 = GetItem(p, 5);
            dm.Player.Inventory.Trinket = GetItem(p, 6);
            #endregion
        }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            //empty
        }

        private async void UpdateData(object sender, ElapsedEventArgs e)
        {
            string jsonData = "";
            try
            {
                using var response = await httpClient.GetAsync(URI);
                if (response.IsSuccessStatusCode)
                {
                    using var content = response.Content;
                    jsonData = await content.ReadAsStringAsync();
                }
            }
            catch
            {
                allGameData = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
            {
                allGameData = null;
                return;
            }

            allGameData = JsonConvert.DeserializeObject<_RootGameData>(jsonData);
        }

        private static ItemSlotDataModel GetItem(_AllPlayer p, int slot)
        {
            _Item newItem = p.items.FirstOrDefault(item => item.slot == slot);

            if (newItem == null)
            {
                return new ItemSlotDataModel();
            }
            else
            {
                return new ItemSlotDataModel(newItem);
            }
        }

        public static TEnum TryParseOr<TEnum>(string value, bool ignoreCase, TEnum defaultValue) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, ignoreCase, out TEnum res))
            {
                return res;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
