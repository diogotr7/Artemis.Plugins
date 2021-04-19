using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class HardwareMonitorDataModel : DataModel { }

    public class HardwareDynamicDataModel : DataModel { }

    public class SensorTypeDynamicDataModel : DataModel { }

    public class SensorDynamicDataModel : DataModel
    {
        [DataModelIgnore]
        internal string Identifier { get; set; }

        public virtual float CurrentValue { get; set; }
        public virtual float Minimum { get; set; }
        public virtual float Maximum { get; set; }

        public SensorDynamicDataModel(string id)
        {
            Identifier = id;
        }
    }
}
