using System;
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