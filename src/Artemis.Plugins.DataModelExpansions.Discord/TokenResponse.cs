using Newtonsoft.Json;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }

        public long ExpiresIn { get; set; }

        public string RefreshToken { get; set; }

        public string Scope { get; set; }

        public string TokenType { get; set; }
    }
}