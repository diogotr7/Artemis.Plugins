namespace Artemis.Plugins.Modules.Discord.DataModels;

public enum DiscordVoiceChannelState
{
    /// <summary>
    /// TCP disconnected
    /// </summary>
    DISCONNECTED,
    /// <summary>
    /// Waiting for voice endpoint
    /// </summary>
    AWAITING_ENDPOINT,

    /// <summary>
    /// TCP authenticating
    /// </summary>
    AUTHENTICATING,

    /// <summary>
    /// TCP connecting
    /// </summary>
    CONNECTING,//TCP connecting

    /// <summary>
    /// TCP connected
    /// </summary>
    CONNECTED,

    /// <summary>
    /// TCP connected, Voice disconnected
    /// </summary>
    VOICE_DISCONNECTED,

    /// <summary>
    /// TCP connected, Voice connecting
    /// </summary>
    VOICE_CONNECTING,

    /// <summary>
    /// TCP connected, Voice connected
    /// </summary>
    VOICE_CONNECTED,

    /// <summary>
    /// No route to host
    /// </summary>
    NO_ROUTE,

    /// <summary>
    /// WebRTC ice checking
    /// </summary>
    ICE_CHECKING,
}