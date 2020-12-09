using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels;
using Artemis.Plugins.Modules.LeagueOfLegends.DataModels.Enums;
using Artemis.Plugins.Modules.LeagueOfLegends.GameData;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private float _lastEventTime;
        private readonly PluginSetting<Dictionary<Champion, SKColor>> _colors;

        public LeagueOfLegendsModule(PluginSettings settings)
        {
            _colors = settings.GetSetting("ChampionColors", new Dictionary<Champion, SKColor>(DefaultChampionColors.Colors));
        }

        public override void Enable()
        {
            DisplayName = "League Of Legends";
            DisplayIcon = "LeagueOfLegendsIcon.svg";
            DefaultPriorityCategory = ModulePriorityCategory.Application;
            ActivationRequirements.Add(new ProcessActivationRequirement("League Of Legends"));

            //mock live game api
            //ActivationRequirements.Add(new ProcessActivationRequirement("node"));

            httpClientHandler = new HttpClientHandler
            {
                //we need this to not make the user install Riot's certificate on their computer
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromMilliseconds(80);

            UpdateDuringActivationOverride = false;
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
            if (!isOverride)
                httpClient?.CancelPendingRequests();
        }

        public override void Update(double deltaTime) { }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo) { }

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
    }
}