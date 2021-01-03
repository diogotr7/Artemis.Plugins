namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class AuthorizeDiscordResponse : CommandDiscordResponse
    {
        public AuthorizeData Data { get; set; }
    }

    public class AuthorizeData
    {
        public string Code { get; set; }
    }
}