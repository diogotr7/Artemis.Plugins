using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class HardwareMonitorDataModel : DataModel
    {
        public int Dummy { get; set; } = 69;
    }

    public class HardwareDynamicDataModel : DataModel
    {
    }

    public class SensorDynamicDataModel : DataModel
    {
        public float SensorValue { get; set; }

        public override string ToString()
        {
            return $"SensorValue: {SensorValue}";
        }
    }
}
