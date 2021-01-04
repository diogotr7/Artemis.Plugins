using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Serialization;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class SavedToken
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("expiration_date")]
        public DateTime ExpirationDate;
    }
}