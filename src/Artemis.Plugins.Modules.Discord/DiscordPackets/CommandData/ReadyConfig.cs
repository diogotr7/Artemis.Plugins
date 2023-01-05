namespace Artemis.Plugins.Modules.Discord.DiscordPackets.CommandData;

public record ReadyConfig
(
    string CdnHost,
    string ApiEndpoint,
    string Enviroment
);
