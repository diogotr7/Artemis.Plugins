using System;

namespace Artemis.Plugins.Modules.Discord
{
    public record NotificationCreateData
    (
        string ChannelId,
        DiscordMessage Message,
        string IconUrl,
        string Title,
        string Body
    );

    //TODO: i have no idea what the hell they did with this data structure.
    //The docs don't seem to be correct.
    public record DiscordMessage
    (
        string Id,
        string Content,
        string Nick,
        DateTime Timestamp,
        DateTime? EditedTimestamp,
        bool Tts,
        int Type,
        DiscordUser Author,
        bool Pinned

    //"mentions": [],
    //"mention_roles": [],
    //"embeds": [],
    //"attachments": [],
    //"content_parsed"
    );
}