using System;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public interface IDiscordAuthClient : IDisposable
{
    string ClientId { get; }
    string Origin { get; }
    string? AccessToken { get; }
    Task<TokenResponse> GetAccessTokenAsync(string challengeCode);
}