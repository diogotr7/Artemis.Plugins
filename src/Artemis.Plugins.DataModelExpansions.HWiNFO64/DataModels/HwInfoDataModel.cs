using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.HWiNFO64.DataModels
{
    public class HwInfoDataModel : DataModel { }
    public class HardwareDynamicDataModel : DataModel { }
    public class SensorTypeDynamicDataModel : DataModel { }

    public class SensorDynamicDataModel : DataModel
    {
        public double CurrentValue { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Average { get; set; }
    }
}