using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class AuthenticateDiscordResponse : CommandDiscordResponse
    {
        public AuthenticateData Data { get; set; }
    }

    public class DiscordUser
    {
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string Id { get; set; }
        public string Avatar { get; set; }
        public bool? Bot { get; set; }
    }

    public class DiscordApplication
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public bool Hook { get; set; }
        public List<string> RpcOrigins { get; set; }
        public string VerifyKey { get; set; }
    }

    public class AuthenticateData
    {
        public DiscordUser User { get; set; }
        public DiscordApplication Application { get; set; }
        public DateTime Expires { get; set; }
        public string AccessToken { get; set; }
    }
}
