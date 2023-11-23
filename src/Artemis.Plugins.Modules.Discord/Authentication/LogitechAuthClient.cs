using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class LogitechAuthClient : DiscordAuthClientBase
{
    private const string TokenEndpoint = "https://ymj1tb3arf.execute-api.us-east-1.amazonaws.com/prod/create_discord_access_token";

    public LogitechAuthClient(PluginSettings token) : base(token.GetSetting<SavedToken>("DiscordTokenLogitech"))
    {
    }

    public override string ClientId => "227491271223017472";
    public override string Origin => "http://localhost";

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var values = new Dictionary<string, string>
        {
            ["code"] = challengeCode
        };

        using var httpContent = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync(TokenEndpoint, httpContent);
        var responseString = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
        SaveToken(token);
        return token;
    }

    public override async Task RefreshAccessTokenAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["refresh_token"] = Token.Value.RefreshToken
        };

        using var httpContent = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");
        using var response = await HttpClient.PostAsync(TokenEndpoint, httpContent);
        var responseString = await response.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString);
        SaveToken(token);
    }
}