using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class HardwareMonitorDataModel : DataModel
    {
        public UsageTemperaturePowerInfo Cpu { get; set; } = new UsageTemperaturePowerInfo();

        public UsageTemperaturePowerInfo Gpu { get; set; } = new UsageTemperaturePowerInfo();

        public float RamUsed { get; set; }

        public float RamTotal { get; set; }
    }

    public class UsageTemperaturePowerInfo
    {
        [DataModelProperty(Name = "Usage", Affix = "%", MinValue = 0, MaxValue = 100)]
        public float Usage { get; set; }

        [DataModelProperty(Name = "Temperature", Affix = "°C")]
        public float Temperature { get; set; }

        [DataModelProperty(Name = "Power Usage", Affix = "W", MinValue = 0)]
        public float PowerUsage { get; set; }
    }
}
