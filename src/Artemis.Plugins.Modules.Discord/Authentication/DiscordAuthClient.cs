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
    public override string Origin => throw new NotSupportedException("Discord does not support a custom origin");
    private readonly string _clientSecret;

    public DiscordAuthClient(PluginSettings settings) : base(settings.GetSetting<string>("DiscordAccessToken"))
    {
        var clientIdSetting = settings.GetSetting<string>("DiscordClientId");
        var clientSecretSetting = settings.GetSetting<string>("DiscordClientSecret");
        
        if (!AreClientIdAndSecretValid(clientIdSetting, clientSecretSetting))
            throw new InvalidOperationException("Invalid client id or secret. Please check your settings.");
        
        ClientId = clientIdSetting.Value!;
        _clientSecret = clientSecretSetting.Value!;
    }
    
    private static bool AreClientIdAndSecretValid(PluginSetting<string> clientId, PluginSetting<string> clientSecret)
    {
        return clientId.Value?.All(char.IsDigit) == true && clientSecret.Value?.Length > 0;
    }

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        Dictionary<string, string> values = new()
        {
            ["grant_type"] = "authorization_code",
            ["code"] = challengeCode,
            ["client_id"] = ClientId,
            ["client_secret"] = _clientSecret
        };

        using var response = await HttpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
        var responseString = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(responseString);
        }

        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
        SaveToken(token);
        return token;
    }

    // public override async Task RefreshAccessTokenAsync()
    // {
    //     if (!HasToken)
    //         throw new InvalidOperationException("No token to refresh");
    //     
    //     Dictionary<string, string> values = new()
    //     {
    //         ["grant_type"] = "refresh_token",
    //         ["refresh_token"] = Token.Value!.RefreshToken,
    //         ["client_id"] = ClientId,
    //         ["client_secret"] = _clientSecret
    //     };
    //
    //     using var response = await HttpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(values));
    //     var responseString = await response.Content.ReadAsStringAsync();
    //     if (!response.IsSuccessStatusCode)
    //     {
    //         throw new UnauthorizedAccessException(responseString);
    //     }
    //
    //     var token = JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
    //     SaveToken(token);
    // }
}