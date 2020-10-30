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

            DataModel.Player.ChampionColor = _colors.Value[DataModel.Player.Champion];

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
                    case AceEvent ae:
                        DataModel.Match.Ace.Trigger(new AceEventArgs { Acer = ae.Acer, AcingTeam = ae.AcingTeam });
                        break;
                    case BaronKillEvent bke:
                        DataModel.Match.BaronKill.Trigger(new EpicCreatureKillEventArgs { Assisters = bke.Assisters, KillerName = bke.KillerName, Stolen = bke.Stolen });
                        break;
                    case ChampionKillEvent cke:
                        DataModel.Match.ChampionKill.Trigger(new ChampionKillEventArgs { KillerName = cke.KillerName, Assisters = cke.Assisters, VictimName = cke.VictimName});
                        break;
                    case DragonKillEvent dke:
                        DataModel.Match.DragonKill.Trigger(new DragonKillEventArgs { Assisters = dke.Assisters, DragonType = dke.DragonType, KillerName = dke.KillerName, Stolen = dke.Stolen });
                        break;
                    case FirstBloodEvent fbe:
                        DataModel.Match.FirstBlood.Trigger(new FirstBloodEventArgs { Recipient = fbe.Recipient });
                        break;
                    case FirstBrickEvent fbre:
                        DataModel.Match.FirstBrick.Trigger();
                        break;
                    case GameEndEvent gee:
                        DataModel.Match.GameEnd.Trigger(new GameEndEventArgs { Result = gee.Result });
                        break;
                    case GameStartEvent gse:
                        DataModel.Match.GameStart.Trigger();
                        break;
                    case HeraldKillEvent hke:
                        DataModel.Match.HeraldKill.Trigger(new EpicCreatureKillEventArgs { Stolen = hke.Stolen, KillerName = hke.KillerName, Assisters = hke.Assisters });
                        break;
                    case InhibKillEvent ike:
                        DataModel.Match.InhibKill.Trigger(new InhibKillEventArgs { Assisters = ike.Assisters, KillerName = ike.KillerName, InhibKilled = ike.InhibKilled });
                        break;
                    case InhibRespawnedEvent ire:
                        DataModel.Match.InhibRespawned.Trigger(new InhibRespawnedEventArgs { InhibRespawned = ire.InhibRespawned });
                        break;
                    case InhibRespawningSoonEvent irse:
                        DataModel.Match.InhibRespawningSoon.Trigger(new InhibRespawningSoonEventArgs { InhibRespawningSoon = irse.InhibRespawningSoon });
                        break;
                    case MinionsSpawningEvent mse:
                        DataModel.Match.MinionsSpawning.Trigger();
                        break;
                    case MultikillEvent mke:
                        DataModel.Match.Multikill.Trigger(new MultikillEventArgs { KillerName = mke.KillerName, KillStreak =  mke.KillStreak});
                        break;
                    case TurretKillEvent tke:
                        DataModel.Match.TurretKill.Trigger(new TurretKillEventArgs { KillerName = tke.KillerName, Assisters = tke.Assisters, TurretKilled = tke.TurretKilled });
                        break;
                }
            }

            _lastEventTime = DataModel.RootGameData.Events.Events.Last().EventTime;
            #endregion
        }
    }
}