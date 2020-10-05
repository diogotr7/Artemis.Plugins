using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class HardwareMonitorDataModel : DataModel { }

    public class HardwareDynamicDataModel : DataModel { }

    public class SensorTypeDynamicDataModel : DataModel { }

    public class SensorDynamicDataModel : DataModel
    {
        [DataModelProperty(Affix = "ASD")]
        public float CurrentValue { get; set; }

        public float Minimum { get; set; }

        public float Maximum { get; set; }

        public override string ToString()
        {
            return $"SensorValue: {CurrentValue}";
        }
    }
}
