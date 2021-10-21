using System;

namespace Artemis.Plugins.Modules.Discord
{
    //TODO: i have no idea what the hell they did with this data structure.
    //The docs don't seem to be correct.
    public record Message
    (
        string Id,
        string Content,
        string Nick,
        DateTime Timestamp,
        DateTime? EditedTimestamp,
        bool Tts,
        int Type,
        User Author,
        bool Pinned

    //"mentions": [],
    //"mention_roles": [],
    //"embeds": [],
    //"attachments": [],
    //"content_parsed"
    );
}