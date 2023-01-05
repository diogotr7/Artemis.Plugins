using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Artemis.Plugins.Modules.Discord.Authentication;

#pragma warning disable CS8618

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TokenResponse
{
    public string AccessToken { get; set; }
    public long ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
    public string TokenType { get; set; }
}