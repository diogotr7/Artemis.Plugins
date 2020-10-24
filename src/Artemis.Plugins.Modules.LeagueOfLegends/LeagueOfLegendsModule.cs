using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsConfigurationDialog;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsModule : ProfileModule<LeagueOfLegendsDataModel>
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private _RootGameData allGameData;

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

        public override void Update(double deltaTime)
        {
            LeagueOfLegendsDataModel dm = DataModel;

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

            List<_DragonKillEvent> drags = allGameData.events.Events.OfType<_DragonKillEvent>().ToList();

            dm.Match.CloudDragonsKilled = drags.Count(d => d.DragonType == "Air");
            dm.Match.MountainDragonsKilled = drags.Count(d => d.DragonType == "Earth");
            dm.Match.InfernalDragonsKilled = drags.Count(d => d.DragonType == "Fire");
            dm.Match.OceanDragonsKilled = drags.Count(d => d.DragonType == "Water");
            dm.Match.ElderDragonsKilled = drags.Count(d => d.DragonType == "Elder");
            dm.Match.DragonsKilled = drags.Count;

            dm.Match.BaronsKilled = allGameData.events.Events.Count(ev => ev is _BaronKillEvent);
            dm.Match.HeraldsKilled = allGameData.events.Events.Count(ev => ev is _HeraldKillEvent);
            dm.Match.TurretsKilled = allGameData.events.Events.Count(ev => ev is _TurretKillEvent);
            dm.Match.InhibsKilled = allGameData.events.Events.Count(ev => ev is _InhibKillEvent);
            dm.Match.MapTerrain = TryParseOr(allGameData.gameData.mapTerrain, true, MapTerrain.Unknown);
            #endregion

            #region Player
            _ActivePlayer ap = allGameData.activePlayer;
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

            _AllPlayer p = allGameData.allPlayers.FirstOrDefault(a => a.summonerName == ap.summonerName);
            if (p == null)
            {
                return;
            }

            dm.Player.Champion = TryParseOr(p.championName, true, Champion.Unknown);
            dm.Player.ChampionColor = _colors.Value[dm.Player.Champion];
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

        private async void UpdateData(double deltaTime)
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

            return newItem == null ? new ItemSlotDataModel() : new ItemSlotDataModel(newItem);
        }

        private static TEnum TryParseOr<TEnum>(string value, bool ignoreCase, TEnum defaultValue) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, ignoreCase, out TEnum res))
                return res;
            else if (ParseEnum<TEnum>.TryParse(value, out TEnum oof))
                return oof;
            else
                return defaultValue;
        }
    }

    //adapted from https://stackoverflow.com/questions/30526757
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    internal class NameAttribute : Attribute
    {
        public readonly string[] Names;

        public NameAttribute(params string[] names)
        {
            if (names?.Any(x => x == null) ?? false)
            {
                throw new ArgumentNullException(nameof(names));
            }

            Names = names.Distinct().ToArray();
        }
    }

    internal static class ParseEnum<TEnum> where TEnum : struct, Enum
    {
        internal static readonly Dictionary<string, TEnum> Values = new Dictionary<string, TEnum>();

        static ParseEnum()
        {
            Values = typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(a => new { NameAtt = a.GetCustomAttribute<NameAttribute>(), EnumValue = (TEnum)a.GetValue(null) })
                .Where(a => a.NameAtt != null)
                .SelectMany(field => field.NameAtt.Names, (field, name) => new { Key = name, Value = field.EnumValue })
                .ToDictionary(a => a.Key, a => a.Value);
        }

        internal static bool TryParse(string value, out TEnum result) => Values.TryGetValue(value, out result);
    }
}