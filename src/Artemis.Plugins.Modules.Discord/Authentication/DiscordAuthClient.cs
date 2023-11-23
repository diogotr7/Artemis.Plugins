using Artemis.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class DiscordAuthClient : DiscordAuthClientBase
{
    public override string ClientId { get; }
    private readonly string _clientSecret;
    private readonly HttpClient _httpClient;

    public DiscordAuthClient(PluginSettings settings) : base(settings.GetSetting<SavedToken>("DiscordToken"))
    {
        var clientIdSetting = settings.GetSetting<string>("DiscordClientId");
        var clientSecretSetting = settings.GetSetting<string>("DiscordClientSecret");
        
        if (!AreClientIdAndSecretValid(clientIdSetting, clientSecretSetting))
            throw new InvalidOperationException("Invalid client id or secret. Please check your settings.");
        
        ClientId = clientIdSetting.Value!;
        _clientSecret = clientSecretSetting.Value!;
        _httpClient = new();
    }
    
    private bool AreClientIdAndSecretValid(PluginSetting<string> clientId, PluginSetting<string> clientSecret)
    {
        return clientId.Value?.All(c => char.IsDigit(c)) == true && clientSecret.Value?.Length > 0;
    }

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var token = await GetCredentialsAsync("authorization_code", "code", challengeCode);
        SaveToken(token);
        return token;
    }

    public override async Task RefreshAccessTokenAsync()
    {
        if (!HasToken)
            throw new InvalidOperationException("No token to refresh");
        
        TokenResponse token = await GetCredentialsAsync("refresh_token", "refresh_token", Token.Value!.RefreshToken);
        SaveToken(token);
    }

    private async Task<TokenResponse> GetCredentialsAsync(string grantType, string secretType, string secret)
    {
        Dictionary<string, string> values = new()
        {
            ["grant_type"] = grantType,
            [secretType] = secret,
            ["client_id"] = ClientId,
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
    
    public override void Dispose()
    {
        _httpClient.Dispose();
    }
}