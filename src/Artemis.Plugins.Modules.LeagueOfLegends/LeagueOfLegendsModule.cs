using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using Newtonsoft.Json;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    [PluginFeature(AlwaysEnabled = true, Icon = "LeagueOfLegendsIcon.svg", Name = "League Of Legends")]
    public class LeagueOfLegendsModule : Module<LeagueOfLegendsDataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new() { new ProcessActivationRequirement("League Of Legends") };

        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private const string champPortraitURIStringToFormat = "http://ddragon.leagueoflegends.com/cdn/img/champion/tiles/{0}.jpg";
        private readonly IColorQuantizerService _colorQuantizer;
        private readonly ILogger _logger;

        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;
        private float _lastEventTime;
        private readonly PluginSetting<Dictionary<Champion, SKColor>> _colors;
        private readonly ConcurrentDictionary<string, ChampionColorsDataModel> championColorCache
                    = new ConcurrentDictionary<string, ChampionColorsDataModel>();

        public LeagueOfLegendsModule(PluginSettings settings, ILogger logger, IColorQuantizerService colorQuantizer)
        {
            UpdateDuringActivationOverride = false;

            _logger = logger;
            //create a copy here
            _colors = settings.GetSetting("ChampionColors", DefaultChampionColors.GetNewDictionary());
            DefaultChampionColors.EnsureAllChampionsPresent(_colors.Value);
            AddDefaultProfile(DefaultCategoryName.Games, Path.Combine("Profiles", "Default.json"));
            _colorQuantizer = colorQuantizer;
        }

        public override void Enable()
        {
            httpClientHandler = new HttpClientHandler
            {
                //we need this to not make the user install Riot's certificate on their computer
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(1500);
            DataModel.Player.colorDictionary = _colors.Value;
            AddTimedUpdate(TimeSpan.FromMilliseconds(100), UpdateData);
        }

        public override void Disable()
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();
        }

        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            DataModel.Player.ChampionColors = new ChampionColorsDataModel();
            if (!isOverride)
                httpClient?.CancelPendingRequests();
        }

        public override void Update(double deltaTime) { }

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
                DataModel.RootGameData = default;
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
            {
                DataModel.RootGameData = default;
                return;
            }

            DataModel.RootGameData = JsonConvert.DeserializeObject<RootGameData>(jsonData);

            try
            {
                await UpdateChampionColors(DataModel.Player.InternalChampionName, DataModel.Player.SkinID);
            }
            catch
            {
                return;
            }

            #region events
            if (DataModel.RootGameData.Events.Events.Length == 0)
            {
                _lastEventTime = 0f;
                return;
            }

            foreach (LolEvent e in DataModel.RootGameData.Events.Events.Where(ev => ev.EventTime > _lastEventTime))
            {
                switch (e)
                {
                    case AceEvent aceEvent:
                        DataModel.Match.Ace.Trigger(new AceEventArgs
                        {
                            Acer = aceEvent.Acer,
                            AcingTeam = ParseEnum<Team>.TryParseOr(aceEvent.AcingTeam, Team.Unknown)
                        });
                        break;
                    case BaronKillEvent baronKillEvent:
                        DataModel.Match.BaronKill.Trigger(new EpicCreatureKillEventArgs
                        {
                            Assisters = baronKillEvent.Assisters,
                            KillerName = baronKillEvent.KillerName,
                            Stolen = baronKillEvent.Stolen
                        });
                        break;
                    case ChampionKillEvent championKillEvent:
                        DataModel.Match.ChampionKill.Trigger(new ChampionKillEventArgs
                        {
                            KillerName = championKillEvent.KillerName,
                            Assisters = championKillEvent.Assisters,
                            VictimName = championKillEvent.VictimName
                        });
                        break;
                    case DragonKillEvent dragonKillEvent:
                        DataModel.Match.DragonKill.Trigger(new DragonKillEventArgs
                        {
                            Assisters = dragonKillEvent.Assisters,
                            DragonType = ParseEnum<DragonType>.TryParseOr(dragonKillEvent.DragonType, DragonType.Unknown),
                            KillerName = dragonKillEvent.KillerName,
                            Stolen = dragonKillEvent.Stolen
                        });
                        break;
                    case FirstBloodEvent firstBloodEvent:
                        DataModel.Match.FirstBlood.Trigger(new FirstBloodEventArgs
                        {
                            Recipient = firstBloodEvent.Recipient
                        });
                        break;
                    case FirstBrickEvent firstBrickEvent:
                        DataModel.Match.FirstBrick.Trigger();
                        break;
                    case GameEndEvent gameEndEvent:
                        DataModel.Match.GameEnd.Trigger(new GameEndEventArgs
                        {
                            Win = gameEndEvent.Result == "Win"
                        });
                        break;
                    case GameStartEvent gameStartEvent:
                        DataModel.Match.GameStart.Trigger();
                        break;
                    case HeraldKillEvent heraldKillEvent:
                        DataModel.Match.HeraldKill.Trigger(new EpicCreatureKillEventArgs
                        {
                            Stolen = heraldKillEvent.Stolen,
                            KillerName = heraldKillEvent.KillerName,
                            Assisters = heraldKillEvent.Assisters
                        });
                        break;
                    case InhibKillEvent inhibKillEvent:
                        DataModel.Match.InhibKill.Trigger(new InhibKillEventArgs
                        {
                            Assisters = inhibKillEvent.Assisters,
                            KillerName = inhibKillEvent.KillerName,
                            InhibKilled = ParseEnum<Inhibitor>.TryParseOr(inhibKillEvent.InhibKilled, Inhibitor.Unknown)
                        });
                        break;
                    case InhibRespawnedEvent inhibRespawnedEvent:
                        DataModel.Match.InhibRespawned.Trigger(new InhibRespawnedEventArgs
                        {
                            InhibRespawned = ParseEnum<Inhibitor>.TryParseOr(inhibRespawnedEvent.InhibRespawned, Inhibitor.Unknown)
                        });
                        break;
                    case InhibRespawningSoonEvent inhibRespawningSoonEvent:
                        DataModel.Match.InhibRespawningSoon.Trigger(new InhibRespawningSoonEventArgs
                        {
                            InhibRespawningSoon = ParseEnum<Inhibitor>.TryParseOr(inhibRespawningSoonEvent.InhibRespawningSoon, Inhibitor.Unknown)
                        });
                        break;
                    case MinionsSpawningEvent minionsSpawningEvent:
                        DataModel.Match.MinionsSpawning.Trigger();
                        break;
                    case MultikillEvent multikillEvent:
                        DataModel.Match.Multikill.Trigger(new MultikillEventArgs
                        {
                            KillerName = multikillEvent.KillerName,
                            KillStreak = multikillEvent.KillStreak
                        });
                        break;
                    case TurretKillEvent turretKillEvent:
                        DataModel.Match.TurretKill.Trigger(new TurretKillEventArgs
                        {
                            KillerName = turretKillEvent.KillerName,
                            Assisters = turretKillEvent.Assisters,
                            TurretKilled = ParseEnum<Turret>.TryParseOr(turretKillEvent.TurretKilled, Turret.Unknown)
                        });
                        break;
                }

                Console.WriteLine();
            }

            _lastEventTime = DataModel.RootGameData.Events.Events.Last().EventTime;
            #endregion
        }
        private async Task UpdateChampionColors(string internalChampionName, int skinId)
        {
            string champSkinKey = internalChampionName + "_" + skinId;
            string formattedChampPortraitURIString = String.Format(champPortraitURIStringToFormat, champSkinKey);
            if (!championColorCache.ContainsKey(champSkinKey))
            {
                try
                {
                    using HttpResponseMessage response = await httpClient.GetAsync(formattedChampPortraitURIString);
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using SKBitmap skbm = SKBitmap.Decode(stream);
                    SKColor[] skClrs = _colorQuantizer.Quantize(skbm.Pixels, 256);
                    championColorCache[champSkinKey] = new ChampionColorsDataModel
                    {
                        Default = _colors.Value[ParseEnum<Champion>.TryParseOr(DataModel.Player.Champion, Champion.Unknown)], // This is just to keep the manually created enum available just in case.
                        Vibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.Vibrant, true),
                        LightVibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.LightVibrant, true),
                        DarkVibrant = _colorQuantizer.FindColorVariation(skClrs, ColorType.DarkVibrant, true),
                        Muted = _colorQuantizer.FindColorVariation(skClrs, ColorType.Muted, true),
                        LightMuted = _colorQuantizer.FindColorVariation(skClrs, ColorType.LightMuted, true),
                        DarkMuted = _colorQuantizer.FindColorVariation(skClrs, ColorType.DarkMuted, true),
                    };
                }
                catch (Exception exception)
                {
                    _logger.Error("Failed to get champion art colors: " + formattedChampPortraitURIString + "\n" + exception.ToString());
                    throw;
                }
            }

            if (championColorCache.TryGetValue(champSkinKey, out var colorDataModel))
                DataModel.Player.ChampionColors = colorDataModel;
        }
    }
}