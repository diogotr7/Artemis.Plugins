using Artemis.Core.Modules;

namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    public class Fallout4DataModel : DataModel
    {
        public StatusDataModel Status { get; set; } = new StatusDataModel();
        public SpecialDataModel Special { get; set; } = new SpecialDataModel();
        public StatsDataModel Stats { get; set; } = new StatsDataModel();
        public PlayerInfoDataModel Player { get; set; } = new PlayerInfoDataModel();
    }
}