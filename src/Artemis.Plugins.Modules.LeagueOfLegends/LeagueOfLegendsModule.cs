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
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public class LeagueOfLegendsModule : ProfileModule<LeagueOfLegendsDataModel>
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;

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
        }
    }
}