using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.EventArgs;
using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.Utils;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    [PluginFeature(AlwaysEnabled = true, Icon = "LeagueOfLegendsIcon.svg", Name = "League Of Legends")]
    public class LeagueOfLegendsModule : Module<LeagueOfLegendsDataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new() { new ProcessActivationRequirement("League Of Legends") };

        private readonly ConcurrentDictionary<string, ChampionColorsDataModel> championColorCache = new();
        private readonly PluginSetting<Dictionary<Champion, SKColor>> _colors;
        private readonly IColorQuantizerService _colorQuantizer;
        private readonly ILogger _logger;

        private LolClient lolClient;
        private HttpClient httpClient;
        private float _lastEventTime;

        public LeagueOfLegendsModule(PluginSettings settings, ILogger logger, IColorQuantizerService colorQuantizer)
        {
            _logger = logger;
            _colorQuantizer = colorQuantizer;
            _colors = settings.GetSetting("ChampionColors", DefaultChampionColors.GetNewDictionary());
            DefaultChampionColors.EnsureAllChampionsPresent(_colors.Value);

            UpdateDuringActivationOverride = false;
            AddDefaultProfile(DefaultCategoryName.Games, Path.Combine("Profiles", "Default.json"));
        }

        public override void Enable()
        {
            lolClient = new LolClient();
            httpClient = new HttpClient();
            DataModel.Player.colorDictionary = _colors.Value;
            AddTimedUpdate(TimeSpan.FromMilliseconds(100), UpdateData);
        }

        public override void Disable()
        {
            lolClient?.Dispose();
            httpClient?.Dispose();
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
            try
            {
                DataModel.RootGameData = await lolClient.GetAllDataAsync();
            }
            catch
            {
                DataModel.RootGameData = new();
                return;
            }

            try
            {
                await UpdateChampionColors(DataModel.Player.InternalChampionName, DataModel.Player.SkinID);
            }
            catch
            {
                return;
            }

            FireOffEvents();
        }

        private void FireOffEvents()
        {
            if (DataModel.RootGameData.Events.Events.Length == 0)
            {
                _lastEventTime = 0f;
                return;
            }

            foreach (LolEvent e in DataModel.RootGameData.Events.Events)
            {
                if (e.EventTime <= _lastEventTime)
                    continue;

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

                _lastEventTime = e.EventTime;
            }
        }

        private async Task UpdateChampionColors(string internalChampionName, int skinId)
        {
            string champSkinKey = $"{internalChampionName}_{skinId}";
            string champSkinUri = $"http://ddragon.leagueoflegends.com/cdn/img/champion/tiles/{champSkinKey}.jpg";
            if (!championColorCache.ContainsKey(champSkinKey))
            {
                try
                {
                    using HttpResponseMessage response = await httpClient.GetAsync(champSkinUri);
                    using Stream stream = await response.Content.ReadAsStreamAsync();
                    using SKBitmap skbm = SKBitmap.Decode(stream);
                    SKColor[] skClrs = _colorQuantizer.Quantize(skbm.Pixels, 256);
                    championColorCache[champSkinKey] = new ChampionColorsDataModel
                    {
                        Default = _colors.Value[DataModel.Player.Champion],
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
                    _logger.Error("Failed to get champion art colors: " + champSkinUri + "\n" + exception.ToString());
                    throw;
                }
            }

            if (championColorCache.TryGetValue(champSkinKey, out var colorDataModel))
                DataModel.Player.ChampionColors = colorDataModel;
        }
    }
}