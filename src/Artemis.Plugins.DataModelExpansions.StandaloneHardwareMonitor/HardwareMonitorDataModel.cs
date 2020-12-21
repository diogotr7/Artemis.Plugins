using Artemis.Core.DataModelExpansions;
using LibreHardwareMonitor.Hardware;
using System.Collections.Generic;

namespace Artemis.Plugins.DataModelExpansions.StandaloneHardwareMonitor
{
    public class HardwareMonitorDataModel : DataModel { }

    public class HardwareDynamicDataModel : DataModel { }

    public class SensorTypeDynamicDataModel : DataModel { }

    public class SensorDynamicDataModel : DataModel
    {
        [DataModelIgnore]
        internal ISensor Sensor { get; set; }

        public virtual float CurrentValue => Sensor.Value ?? 0;
        public virtual float Minimum => Sensor.Min ?? 0;
        public virtual float Maximum => Sensor.Max ?? 0;

        public SensorDynamicDataModel(ISensor s)
        {
            Sensor = s;
        }
    }

    public class TemperatureDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "°C")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "°C")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "°C")]
        public override float Maximum => base.Maximum;

        public TemperatureDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class PercentageDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "%")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "%")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "%")]
        public override float Maximum => base.Maximum;

        public PercentageDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class VoltageDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "V")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "V")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "V")]
        public override float Maximum => base.Maximum;

        public VoltageDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class SmallDataDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "MB")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "MB")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "MB")]
        public override float Maximum => base.Maximum;

        public SmallDataDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class BigDataDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "GB")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "GB")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "GB")]
        public override float Maximum => base.Maximum;

        public BigDataDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class PowerDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "W")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "W")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "W")]
        public override float Maximum => base.Maximum;

        public PowerDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class FanDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "RPM")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "RPM")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "RPM")]
        public override float Maximum => base.Maximum;

        public FanDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class ThroughputDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "B/s")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "B/s")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "B/s")]
        public override float Maximum => base.Maximum;

        public ThroughputDynamicDataModel(ISensor id) : base(id)
        {
        }
    }

    public class ClockDynamicDataModel : SensorDynamicDataModel
    {
        [DataModelProperty(Affix = "MHz")]
        public override float CurrentValue => base.CurrentValue;
        [DataModelProperty(Affix = "MHz")]
        public override float Minimum => base.Minimum;
        [DataModelProperty(Affix = "MHz")]
        public override float Maximum => base.Maximum;

        public ClockDynamicDataModel(ISensor id) : base(id)
        {
        }
    }
}