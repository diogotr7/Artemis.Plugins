using System.ComponentModel;

namespace Artemis.Plugins.Modules.Discord.DiscordPluginConfiguration;

public enum DiscordRpcProvider
{
    //streamkit is the default
    [Description("StreamKit (Recommended)")]
    StreamKit = 0,
    Custom,
    Razer,
    Steelseries,
    Logitech,
}