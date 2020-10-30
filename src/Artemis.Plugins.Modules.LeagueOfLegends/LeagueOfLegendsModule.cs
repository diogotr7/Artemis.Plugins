using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsModule : ProfileModule<LeagueOfLegendsDataModel>
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private RootGameData allGameData;
        private float _lastEventTime;

        private readonly PluginSetting<Dictionary<Champion, SKColor>> _colors;

        public LeagueOfLegendsModule(PluginSettings settings)
        {
            _colors = settings.GetSetting("ChampionColors", DefaultChampionColors.Colors);
        }

        public override void EnablePlugin()
        {
            ConfigurationDialog = new PluginConfigurationDialog<LeagueOfLegendsConfigurationDialogViewModel>();
            DisplayName = "League Of Legends";
            DisplayIcon = "LeagueOfLegendsIcon.svg";
            DefaultPriorityCategory = ModulePriorityCategory.Application;
            ActivationRequirements.Add(new ProcessActivationRequirement("League Of Legends"));
            DataModel.Reset();

            httpClientHandler = new HttpClientHandler
            {
                //we need this to not make the user install Riot's certificate on their computer
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(80);
            UpdateDuringActivationOverride = false;
            AddTimedUpdate(TimeSpan.FromMilliseconds(100), UpdateData);
        }

        public override void DisablePlugin()
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
            allGameData = null;
        }

        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            if (!isOverride)
                httpClient?.CancelPendingRequests();
        }

        public override void Update(double deltaTime) { }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo) {  }

        private async Task UpdateData(double deltaTime)
        {
            string jsonData = "";

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(URI);
                if (response.IsSuccessStatusCode)
                {
                    using HttpContent content = response.Content;
                    jsonData = await content.ReadAsStringAsync();
                }
            }
            catch
            {
                allGameData = null;
                DataModel.Reset();
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
            {
                allGameData = null;
                DataModel.Reset();
                return;
            }

            allGameData = JsonConvert.DeserializeObject<RootGameData>(jsonData);

            #region events
            foreach(var e in allGameData.Events.Events.Where(ev => ev.EventTime > _lastEventTime))
            {
                switch (e)
                {
                    //invoke events here with specific eventargs
                    case AceEvent ae:
                        break;
                    case BaronKillEvent bke:
                        break;
                    case ChampionKillEvent cke:
                        break;
                    case DragonKillEvent dke:
                        break;
                    case FirstBloodEvent fbe:
                        break;
                    case FirstBrickEvent fbre:
                        break;
                    case GameEndEvent gee:
                        break;
                    case GameStartEvent gse:
                        break;
                    case HeraldKillEvent hke:
                        break;
                    case InhibKillEvent ike:
                        break;
                    case InhibRespawnedEvent ire:
                        break;
                    case InhibRespawningSoonEvent irse:
                        break;
                    case MinionsSpawningEvent mse:
                        break;
                    case MultikillEvent mke:
                        break;
                    case TurretKillEvent tke:
                        break;
                }
            }
            if (allGameData.Events.Events.Any())
                _lastEventTime = allGameData.Events.Events.Max(ev => ev.EventTime);
            else
                _lastEventTime = 0f;
            #endregion

            LeagueOfLegendsDataModel dm = DataModel;

            #region Match
            dm.Match.InGame = true;
            dm.Match.GameMode = TryParseOr(allGameData.GameData.GameMode, GameMode.Unknown);
            dm.Match.GameTime = allGameData.GameData.GameTime;
            dm.Match.MapTerrain = TryParseOr(allGameData.GameData.MapTerrain, MapTerrain.Unknown);
            #endregion

            #region Player
            ActivePlayer ap = allGameData.ActivePlayer;
            dm.Player.Level = ap.Level;
            dm.Player.Gold = ap.CurrentGold;
            dm.Player.SummonerName = ap.SummonerName;

            dm.Player.Abilities.Q.Level = ap.Abilities.Q.AbilityLevel;
            dm.Player.Abilities.Q.Name = ap.Abilities.Q.DisplayName;
            dm.Player.Abilities.W.Level = ap.Abilities.W.AbilityLevel;
            dm.Player.Abilities.W.Name = ap.Abilities.W.DisplayName;
            dm.Player.Abilities.E.Level = ap.Abilities.E.AbilityLevel;
            dm.Player.Abilities.E.Name = ap.Abilities.E.DisplayName;
            dm.Player.Abilities.R.Level = ap.Abilities.R.AbilityLevel;
            dm.Player.Abilities.R.Name = ap.Abilities.R.DisplayName;

            dm.Player.ChampionStats.AbilityPower = ap.ChampionStats.AbilityPower;
            dm.Player.ChampionStats.Armor = ap.ChampionStats.Armor;
            dm.Player.ChampionStats.ArmorPenetrationFlat = ap.ChampionStats.ArmorPenetrationFlat;
            dm.Player.ChampionStats.ArmorPenetrationPercent = ap.ChampionStats.ArmorPenetrationPercent;
            dm.Player.ChampionStats.AttackDamage = ap.ChampionStats.AttackDamage;
            dm.Player.ChampionStats.AttackRange = ap.ChampionStats.AttackRange;
            dm.Player.ChampionStats.AttackSpeed = ap.ChampionStats.AttackSpeed;
            dm.Player.ChampionStats.BonusArmorPenetrationPercent = ap.ChampionStats.BonusArmorPenetrationPercent;
            dm.Player.ChampionStats.BonusMagicPenetrationPercent = ap.ChampionStats.BonusMagicPenetrationPercent;
            dm.Player.ChampionStats.CooldownReduction = ap.ChampionStats.CooldownReduction;
            dm.Player.ChampionStats.CritChance = ap.ChampionStats.CritChance;
            dm.Player.ChampionStats.CritDamagePercent = ap.ChampionStats.CritDamage;
            dm.Player.ChampionStats.HealthCurrent = ap.ChampionStats.CurrentHealth;
            dm.Player.ChampionStats.HealthRegenRate = ap.ChampionStats.HealthRegenRate;
            dm.Player.ChampionStats.LifeSteal = ap.ChampionStats.LifeSteal;
            dm.Player.ChampionStats.MagicLethality = ap.ChampionStats.MagicLethality;
            dm.Player.ChampionStats.MagicPenetrationFlat = ap.ChampionStats.MagicPenetrationFlat;
            dm.Player.ChampionStats.MagicPenetrationPercent = ap.ChampionStats.BonusMagicPenetrationPercent;
            dm.Player.ChampionStats.MagicResist = ap.ChampionStats.MagicResist;
            dm.Player.ChampionStats.HealthMax = ap.ChampionStats.MaxHealth;
            dm.Player.ChampionStats.MoveSpeed = ap.ChampionStats.MoveSpeed;
            dm.Player.ChampionStats.PhysicalLethality = ap.ChampionStats.PhysicalLethality;
            dm.Player.ChampionStats.ResourceMax = ap.ChampionStats.ResourceMax;
            dm.Player.ChampionStats.ResourceRegenRate = ap.ChampionStats.ResourceRegenRate;
            dm.Player.ChampionStats.ResourceType = TryParseOr(ap.ChampionStats.ResourceType, ResourceType.Unknown);
            dm.Player.ChampionStats.ResourceCurrent = ap.ChampionStats.ResourceValue;
            dm.Player.ChampionStats.SpellVamp = ap.ChampionStats.SpellVamp;
            dm.Player.ChampionStats.Tenacity = ap.ChampionStats.Tenacity;

            AllPlayer p = Array.Find(allGameData.AllPlayers, a => a.SummonerName == ap.SummonerName);
            if (p == null)
            {
                return;
            }

            dm.Player.Champion = TryParseOr(p.ChampionName, Champion.Unknown);
            dm.Player.ChampionColor = _colors.Value[dm.Player.Champion];
            dm.Player.SpellD = TryParseOr(p.SummonerSpells.SummonerSpellOne.DisplayName, DataModels.Enums.SummonerSpell.Unknown);
            dm.Player.SpellF = TryParseOr(p.SummonerSpells.SummonerSpellTwo.DisplayName, DataModels.Enums.SummonerSpell.Unknown);
            dm.Player.Team = TryParseOr(p.Team, Team.Unknown);
            dm.Player.Position = TryParseOr(p.Position, Position.Unknown);

            dm.Player.IsDead = p.IsDead;
            dm.Player.RespawnTimer = p.RespawnTimer;
            dm.Player.Kills = p.Scores.Kills;
            dm.Player.Deaths = p.Scores.Deaths;
            dm.Player.Assists = p.Scores.Assists;
            dm.Player.CreepScore = p.Scores.CreepScore;
            dm.Player.WardScore = p.Scores.WardScore;

            dm.Player.Inventory.Slot1 = GetItem(p, 0);
            dm.Player.Inventory.Slot2 = GetItem(p, 1);
            dm.Player.Inventory.Slot3 = GetItem(p, 2);
            dm.Player.Inventory.Slot4 = GetItem(p, 3);
            dm.Player.Inventory.Slot5 = GetItem(p, 4);
            dm.Player.Inventory.Slot6 = GetItem(p, 5);
            dm.Player.Inventory.Trinket = GetItem(p, 6);
            #endregion
        }

        #region helper methods
        private static ItemSlotDataModel GetItem(AllPlayer p, int slot)
        {
            Item newItem = Array.Find(p.Items, item => item.Slot == slot);

            return newItem == null ? new ItemSlotDataModel() : new ItemSlotDataModel(newItem);
        }

        private static TEnum TryParseOr<TEnum>(string value, TEnum defaultValue, bool ignoreCase = true) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, ignoreCase, out TEnum parseResult))
                return parseResult;
            else if (ParseEnum<TEnum>.TryParse(value, out TEnum customResult))
                return customResult;
            else
                return defaultValue;
        }
        #endregion
    }
}