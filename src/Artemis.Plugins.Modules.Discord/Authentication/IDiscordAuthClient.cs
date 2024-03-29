using System;
using System.Net.Http;
using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public interface IDiscordAuthClient : IDisposable
{
    string ClientId { get; }
    string Origin { get; }
    bool HasToken { get; }
    string AccessToken { get; }
    Task<TokenResponse> GetAccessTokenAsync(string challengeCode);
    Task RefreshAccessTokenAsync();
}

public abstract class DiscordAuthClientBase : IDiscordAuthClient
{
    protected HttpClient HttpClient = new();
    protected readonly PluginSetting<SavedToken> Token;
    protected DiscordAuthClientBase(PluginSetting<SavedToken> token)
    {
        Token = token;
    }
    
    public abstract string ClientId { get; }
    public abstract string Origin { get; }
    public bool HasToken => Token.Value != null;
    public string AccessToken => Token.Value?.AccessToken ?? throw new InvalidOperationException("No token available");
    public abstract Task<TokenResponse> GetAccessTokenAsync(string challengeCode);
    public abstract Task RefreshAccessTokenAsync();
    public void Dispose()
    {
        HttpClient.Dispose();
    }
    
    protected void SaveToken(TokenResponse newToken)
    {
        Token.Value = new SavedToken
        {
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken,
            ExpirationDate = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn)
        };
        Token.Save();
    }
}