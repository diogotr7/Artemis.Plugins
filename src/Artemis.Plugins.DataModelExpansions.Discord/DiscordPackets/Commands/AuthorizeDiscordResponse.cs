namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class AuthorizeDiscordResponse : DiscordResponse
    {
        public AuthorizeData Data { get; set; }
    }

    public class AuthorizeData
    {
        public string Code { get; set; }
    }
}