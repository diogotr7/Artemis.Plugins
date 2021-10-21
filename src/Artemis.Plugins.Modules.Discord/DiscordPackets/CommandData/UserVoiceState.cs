namespace Artemis.Plugins.Modules.Discord
{
    public record UserVoiceState(
        string Nick,
        bool Mute,
        int Volume,
        Pan Pan,
        VoiceState VoiceState,
        User User
    );
}