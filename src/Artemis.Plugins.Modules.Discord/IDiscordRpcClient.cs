using Artemis.Plugins.Modules.Discord.Enums;
using System;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord
{
    public interface IDiscordRpcClient : IDisposable
    {
        event EventHandler<Authenticate> Authenticated;
        event EventHandler<DiscordRpcClientException> Error;
        event EventHandler<Notification> NotificationReceived;
        event EventHandler<SpeakingStartStop> SpeakingStarted;
        event EventHandler<SpeakingStartStop> SpeakingStopped;
        event EventHandler<DiscordEvent> UnhandledEventReceived;
        event EventHandler<VoiceChannelSelect> VoiceChannelUpdated;
        event EventHandler<VoiceConnectionStatus> VoiceConnectionStatusUpdated;
        event EventHandler<VoiceSettings> VoiceSettingsUpdated;
        event EventHandler<UserVoiceState> VoiceStateCreated;
        event EventHandler<UserVoiceState> VoiceStateDeleted;
        event EventHandler<UserVoiceState> VoiceStateUpdated;

        void Connect(int timeoutMs = 500);
        Task<T> GetAsync<T>(DiscordRpcCommand command, params (string Key, object Value)[] parameters) where T : class;
        Task SubscribeAsync(DiscordRpcEvent evt, params (string Key, object Value)[] parameters);
        Task UnsubscribeAsync(DiscordRpcEvent evt, params (string Key, object Value)[] parameters);
    }
}