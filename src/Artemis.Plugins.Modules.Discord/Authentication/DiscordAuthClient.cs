using Artemis.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class DiscordAuthClient : IDisposable
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly PluginSetting<SavedToken> _token;
    private readonly HttpClient _httpClient;

    public DiscordAuthClient(string clientId, string clientSecret, PluginSetting<SavedToken> token)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _token = token;
        _httpClient = new();
    }

    public bool HasToken => _token.Value != null;

    public bool IsTokenValid => HasToken && _token.Value.ExpirationDate >= DateTime.UtcNow;

    public string AccessToken => _token.Value.AccessToken;

    public async Task TryRefreshTokenAsync()
    {
        if (!HasToken)
            return;

        if (_token.Value.ExpirationDate >= DateTime.UtcNow.AddDays(1))
            return;

        await RefreshAccessTokenAsync();
    }

    public async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        TokenResponse token = await GetCredentialsAsync("authorization_code", "code", challengeCode);
        SaveToken(token);
        return token;
    }

    public async Task<TokenResponse> RefreshAccessTokenAsync()
    {
        TokenResponse token = await GetCredentialsAsync("refresh_token", "refresh_token", _token.Value.RefreshToken);
        SaveToken(token);
        return token;
    }

    private async Task<TokenResponse> GetCredentialsAsync(string grantType, string secretType, string secret)
    {
        Dictionary<string, string> values = new()
        {
            ["grant_type"] = grantType,
            [secretType] = secret,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret
        };

        using HttpResponseMessage response = await _httpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
        string responseString = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(responseString);
        }

        return JsonConvert.DeserializeObject<TokenResponse>(responseString);
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
