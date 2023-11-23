using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class SteelseriesAuthClient : DiscordAuthClientBase
{
    private const string TokenEndpoint = "https://id.steelseries.com/discord/auth";
    
    public SteelseriesAuthClient(PluginSettings token) : base(token.GetSetting<SavedToken>("DiscordTokenSteelseries"))
    {
    }

    public override string ClientId => "211138759029293067";
    public override string Origin => "https://steelseries.com";

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var values = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = "authorization_code",
            ["code"] = challengeCode
        };

        using var response = await HttpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(values));
        var responseString = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
        SaveToken(token);
        return token;
    }

    public override async Task RefreshAccessTokenAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = Token.Value.RefreshToken
        };

        using var response = await HttpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(values));
        var responseString = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
        SaveToken(token);
    }
}