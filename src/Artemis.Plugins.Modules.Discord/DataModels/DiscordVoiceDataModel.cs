using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Discord.DataModels;

public class DiscordVoiceDataModel : DataModel
{
    public DataModelEvent Connected { get; } = new();

    public DataModelEvent Disconnected { get; } = new();

    public DiscordVoiceConnectionStatusDataModel Connection { get; } = new();

    public DiscordVoiceSettingsDataModel Settings { get; } = new();

    public DiscordVoiceChannelDataModel Channel { get; } = new();
}
