namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class AuthorizeDiscordResponse : DiscordResponse
    {
        public AuthorizeData Data { get; set; }
    }

    public record AuthorizeData(string Code);
}