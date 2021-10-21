namespace Artemis.Plugins.Modules.Discord
{
    public record VoiceState(
        bool Mute,
        bool Deaf,
        bool SelfMute,
        bool SelfDeaf,
        bool Suppress
    );
}