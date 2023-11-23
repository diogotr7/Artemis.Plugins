using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class DiscordStreamKitAuthClient : IDiscordAuthClient
{
    private readonly PluginSetting<SavedToken> _token;
    private readonly HttpClient _httpClient;

    public DiscordStreamKitAuthClient(PluginSetting<SavedToken> token)
    {
        _token = token;
        _httpClient = new HttpClient();
    }

    public bool HasToken => _token.Value != null;

    public bool IsTokenValid => HasToken && _token.Value!.ExpirationDate >= DateTime.UtcNow;

    public string AccessToken => _token.Value?.AccessToken ?? throw new InvalidOperationException("No token available");

    public async Task RefreshTokenIfNeededAsync()
    {
        if (!HasToken)
            return;

        if (_token.Value!.ExpirationDate >= DateTime.UtcNow.AddDays(1))
            return;

        await RefreshAccessTokenAsync();
    }

    public async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var body = new StringContent(JsonConvert.SerializeObject(new { code = challengeCode }), Encoding.UTF8, "application/json");
        
        using var response = await _httpClient.PostAsync("https://streamkit.discord.com/overlay/token", body);
        
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(responseString);
        }
        
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
        SaveToken(token);
        
        return token;
    }

    public Task RefreshAccessTokenAsync()
    {
        // Streamkit tokens don't support refreshing, or at least I can't find anything about it
        return Task.CompletedTask;
    }

    private void SaveToken(TokenResponse newToken)
    {
        _token.Value = new SavedToken
        {
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken,
            ExpirationDate = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn)
        };
        _token.Save();
    }

    #region IDisposable
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}