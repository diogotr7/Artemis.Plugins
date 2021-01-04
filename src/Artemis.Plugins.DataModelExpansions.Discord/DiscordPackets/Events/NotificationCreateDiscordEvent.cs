using System;

namespace Artemis.Plugins.DataModelExpansions.Discord
{
    internal class NotificationCreateDiscordEvent : DiscordEvent
    {
        public NotificationCreateData Data { get; set; }
    }

    public class NotificationCreateData
    {
        public string ChannelId { get; set; }
        public string IconUrl { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DiscordMessage Message { get; set; }
    }

    public class DiscordMessage
    {
        public string Id { get; set; }
        public bool Blocked { get; set; }
        public string Content { get; set; }
        public string AuthorColor { get; set; }
        public DateTime? EditedTimestamp { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Tts { get; set; }
        public bool MentionEveryone { get; set; }
        public int Type { get; set; }
        public bool Pinned { get; set; }
        public DiscordUser Author { get; set; }
        //"mentions": [],
        //"mention_roles": [],
        //"embeds": [],
        //"attachments": [],
        //"content_parsed"
    }
}