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
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsModule : ProfileModule<LeagueOfLegendsDataModel>
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private RootGameData allGameData;

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
            dm.Match.GameMode = TryParseOr(allGameData.GameData.GameMode, true, GameMode.Unknown);
            dm.Match.GameModeName = allGameData.GameData.GameMode;
            dm.Match.GameTime = allGameData.GameData.GameTime;

            List<DragonKillEvent> drags = allGameData.Events.Events.OfType<DragonKillEvent>().ToList();

            dm.Match.CloudDragonsKilled = drags.Count(d => d.DragonType == "Air");
            dm.Match.MountainDragonsKilled = drags.Count(d => d.DragonType == "Earth");
            dm.Match.InfernalDragonsKilled = drags.Count(d => d.DragonType == "Fire");
            dm.Match.OceanDragonsKilled = drags.Count(d => d.DragonType == "Water");
            dm.Match.ElderDragonsKilled = drags.Count(d => d.DragonType == "Elder");
            dm.Match.DragonsKilled = drags.Count;

            dm.Match.BaronsKilled = allGameData.Events.Events.Count(ev => ev is BaronKillEvent);
            dm.Match.HeraldsKilled = allGameData.Events.Events.Count(ev => ev is HeraldKillEvent);
            dm.Match.TurretsKilled = allGameData.Events.Events.Count(ev => ev is TurretKillEvent);
            dm.Match.InhibsKilled = allGameData.Events.Events.Count(ev => ev is InhibKillEvent);
            dm.Match.MapTerrain = TryParseOr(allGameData.GameData.MapTerrain, true, MapTerrain.Unknown);
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
            dm.Player.ChampionStats.ResourceType = TryParseOr(ap.ChampionStats.ResourceType, true, ResourceType.Unknown);
            dm.Player.ChampionStats.ResourceCurrent = ap.ChampionStats.ResourceValue;
            dm.Player.ChampionStats.SpellVamp = ap.ChampionStats.SpellVamp;
            dm.Player.ChampionStats.Tenacity = ap.ChampionStats.Tenacity;

            AllPlayer p = allGameData.AllPlayers.FirstOrDefault(a => a.SummonerName == ap.SummonerName);
            if (p == null)
            {
                return;
            }

            dm.Player.Champion = TryParseOr(p.ChampionName, true, Champion.Unknown);
            dm.Player.ChampionColor = _colors.Value[dm.Player.Champion];
            dm.Player.SpellD = TryParseOr(p.SummonerSpells.SummonerSpellOne.DisplayName, true, DataModels.Enums.SummonerSpell.Unknown);
            dm.Player.SpellF = TryParseOr(p.SummonerSpells.SummonerSpellTwo.DisplayName, true, DataModels.Enums.SummonerSpell.Unknown);
            dm.Player.Team = TryParseOr(p.Team, true, Team.Unknown);
            dm.Player.Position = TryParseOr(p.Position, true, Position.Unknown);

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

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            //empty
        }

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
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
            {
                allGameData = null;
                return;
            }

            allGameData = JsonConvert.DeserializeObject<RootGameData>(jsonData);
        }

        private static ItemSlotDataModel GetItem(AllPlayer p, int slot)
        {
            Item newItem = Array.Find(p.Items, item => item.Slot == slot);

            return newItem == null ? new ItemSlotDataModel() : new ItemSlotDataModel(newItem);
        }

        private static TEnum TryParseOr<TEnum>(string value, bool ignoreCase, TEnum defaultValue) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, ignoreCase, out TEnum parseResult))
                return parseResult;
            else if (ParseEnum<TEnum>.TryParse(value, out TEnum customResult))
                return customResult;
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