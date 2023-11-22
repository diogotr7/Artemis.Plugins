using Artemis.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public interface IDiscordAuthClient : IDisposable
{
    bool HasToken { get; }
    bool IsTokenValid { get; }
    string AccessToken { get; }
    Task RefreshTokenIfNeededAsync();
    Task<TokenResponse> GetAccessTokenAsync(string challengeCode);
    Task RefreshAccessTokenAsync();
}

public class DiscordAuthClient :  IDiscordAuthClient
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
        var token = await GetCredentialsAsync("authorization_code", "code", challengeCode);
        SaveToken(token);
        return token;
    }

    public async Task RefreshAccessTokenAsync()
    {
        if (!HasToken)
            throw new InvalidOperationException("No token to refresh");
        
        TokenResponse token = await GetCredentialsAsync("refresh_token", "refresh_token", _token.Value!.RefreshToken);
        SaveToken(token);
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

        return JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
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

public class DiscordStreamKitAuthClient : IDiscordAuthClient
{
    private readonly PluginSetting<SavedToken> _token;
    private readonly HttpClient _httpClient;

    public DiscordStreamKitAuthClient(PluginSetting<SavedToken> token)
    {
        _token = token;
        _httpClient = new();
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
        
        using HttpResponseMessage response = await _httpClient.PostAsync("https://streamkit.discord.com/overlay/token", body);
        
        string responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(responseString);
        }
        
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
        SaveToken(token);
        
        return token;
    }

    public async Task RefreshAccessTokenAsync()
    {
        return;
        if (!HasToken)
            throw new InvalidOperationException("No token to refresh");
        
        throw new NotImplementedException();
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
