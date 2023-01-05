using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Artemis.Plugins.Modules.Discord.Authentication;

public class SavedToken
{
    [JsonProperty("refresh_token")]
    public string RefreshToken;

    [JsonProperty("access_token")]
    public string AccessToken;

    [JsonProperty("expiration_date")]
    public DateTime ExpirationDate;
}