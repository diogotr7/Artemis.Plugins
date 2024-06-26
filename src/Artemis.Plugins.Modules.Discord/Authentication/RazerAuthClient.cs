﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class RazerAuthClient : DiscordAuthClientBase
{
    private const string RefreshEndpoint = "https://chroma.razer.com/discord/refreshtoken.php";
    private const string GrantEndpoint = "https://chroma.razer.com/discord/grant.php";
    private const string RedirectUri = "http://chroma.razer.com/discord/";

    public RazerAuthClient(PluginSettings token) : base(token.GetSetting<string>("DiscordAccessTokenRazer"))
    {
    }

    public override string ClientId => "331511201655685132";
    public override string Origin => "http://chroma.razer.com";

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var values = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = "authorization_code",
            ["code"] = challengeCode,
            ["redirect_uri"] = RedirectUri
        };

        using var response = await HttpClient.PostAsync(GrantEndpoint, new FormUrlEncodedContent(values));
        var responseString = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
        SaveToken(token);
        return token;
    }
    
    // public override async Task RefreshAccessTokenAsync()
    // {
    //     var values = new Dictionary<string, string>
    //     {
    //         ["client_id"] = ClientId,
    //         ["grant_type"] = "refresh_token",
    //         ["refresh_token"] = Token.Value.RefreshToken,
    //         ["redirect_uri"] = RedirectUri
    //     };
    //
    //     using var response = await HttpClient.PostAsync(RefreshEndpoint, new FormUrlEncodedContent(values));
    //     var responseString = await response.Content.ReadAsStringAsync();
    //     var tkn = JsonConvert.DeserializeObject<TokenResponse>(responseString);
    //     SaveToken(tkn);
    // }
}