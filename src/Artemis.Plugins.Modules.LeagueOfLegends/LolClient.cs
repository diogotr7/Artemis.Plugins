using Artemis.Plugins.Modules.LeagueOfLegends.GameDataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.LeagueOfLegends
{
    public sealed class LolClient : IDisposable
    {
        private const string BASE_URI = "https://127.0.0.1:2999/liveclientdata";
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpClientHandler;

        public LolClient()
        {
            _httpClientHandler = new HttpClientHandler
            {
                //we need this to not make the user install Riot's certificate on their computer
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            _httpClient = new HttpClient(_httpClientHandler)
            {
                Timeout = TimeSpan.FromMilliseconds(75)
            };
        }

        private async Task<T> GetAndDeserialize<T>(string endpoint)
        {
            string response = await _httpClient.GetStringAsync(endpoint);

            if (string.IsNullOrWhiteSpace(response) || response.Contains("error"))
                throw new Exception("League of Legends returned unexpected response.");

            return JsonConvert.DeserializeObject<T>(response);
        }

        public Task<string> GetActivePlayerNameAsync() => _httpClient.GetStringAsync($"{BASE_URI}/activeplayername");

        public Task<RootGameData> GetAllDataAsync() => GetAndDeserialize<RootGameData>($"{BASE_URI}/allgamedata");

        public Task<ActivePlayer> GetActivePlayerAsync() => GetAndDeserialize<ActivePlayer>($"{BASE_URI}/activeplayer");

        public Task<Abilities> GetActivePlayerAbilitiesAsync() => GetAndDeserialize<Abilities>($"{BASE_URI}/activeplayerabilities");

        public Task<FullRunes> GetActivePlayerRunesAsync() => GetAndDeserialize<FullRunes>($"{BASE_URI}/activeplayerrunes");

        public Task<List<AllPlayer>> GetPlayerListAsync() => GetAndDeserialize<List<AllPlayer>>($"{BASE_URI}/playerlist");

        public Task<List<LolEvent>> GetEventsAsync() => GetAndDeserialize<List<LolEvent>>($"{BASE_URI}/eventdata");

        public Task<GameStats> GetGameStatsAsync() => GetAndDeserialize<GameStats>($"{BASE_URI}/gamestats");

        public Task<Scores> GetPlayerScoresAsync(string summonerName) => GetAndDeserialize<Scores>($"{BASE_URI}/playerscores?summonerName={summonerName}");

        public Task<SummonerSpells> GetPlayerSummonerSpellsAsync(string summonerName)
                        => GetAndDeserialize<SummonerSpells>($"{BASE_URI}/playersummonerspells?summonerName={summonerName}");

        public Task<Runes> GetPlayerRunesAsync(string summonerName)
                        => GetAndDeserialize<Runes>($"{BASE_URI}/playermainrunes?summonerName={summonerName}");

        public Task<List<Item>> GetPlayerItemsAsync(string summonerName)
                        => GetAndDeserialize<List<Item>>($"{BASE_URI}/playeritems?summonerName={summonerName}");

        #region IDisposable
        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                    _httpClientHandler?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
