using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.VoiceMeeter.DataModels
{
    public class VoiceMeeterDataModel : DataModel
    {
        public VoiceMeeterInfoDataModel Information { get; set; } = new();

        public VoiceMeeterStripsDataModel Strips { get; set; } = new();

        public VoiceMeeterBussesDataModel Busses { get; set; } = new();
    }
}