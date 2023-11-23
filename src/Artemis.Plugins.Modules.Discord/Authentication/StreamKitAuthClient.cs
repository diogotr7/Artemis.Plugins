using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Newtonsoft.Json;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class StreamKitAuthClient : DiscordAuthClientBase
{

    public StreamKitAuthClient(PluginSettings token) : base(token.GetSetting<SavedToken>("DiscordTokenStreamKit"))
    {
    }

    public override string ClientId => "207646673902501888";

    public override async Task<TokenResponse> GetAccessTokenAsync(string challengeCode)
    {
        var body = new StringContent(JsonConvert.SerializeObject(new { code = challengeCode }), Encoding.UTF8, "application/json");
        
        using var response = await HttpClient.PostAsync("https://streamkit.discord.com/overlay/token", body);
        
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(responseString);
        }
        
        var token = JsonConvert.DeserializeObject<TokenResponse>(responseString)!;
        SaveToken(token);
        
        return token;
    }

    public override Task RefreshAccessTokenAsync()
    {
        // Streamkit tokens don't support refreshing, or at least I can't find anything about it
        return Task.CompletedTask;
    }
}