using System;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public abstract class DiscordAuthClientBase : IDiscordAuthClient
{
    protected readonly HttpClient HttpClient = new();
    protected readonly PluginSetting<string> Token;
    protected DiscordAuthClientBase(PluginSetting<string> token)
    {
        Token = token;
    }
    
    public abstract string ClientId { get; }
    public abstract string Origin { get; }
    public string? AccessToken => Token.Value;
    public abstract Task<TokenResponse> GetAccessTokenAsync(string challengeCode);

    protected void SaveToken(TokenResponse newToken)
    {
        Token.Value = newToken.AccessToken;
        Token.Save();
    }
    
    public void Dispose()
    {
        HttpClient.Dispose();
    }
}