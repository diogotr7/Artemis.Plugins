namespace Artemis.Plugins.DataModelExpansions.Discord
{
    public class VoiceSettingsUpdateDiscordEvent : DiscordEvent
    {
        public VoiceSettingsData Data { get; set; }
    }
}