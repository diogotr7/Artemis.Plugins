using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class HardwareMonitorDataModelExpansion : DataModelExpansion<HardwareMonitorDataModel>
    {
        private readonly ILogger _logger;

        public HardwareMonitorDataModelExpansion(ILogger logger)
        {
            _logger = logger;
        }

        private static readonly string[] Scopes = new[]
        {
            @"\\.\root\LibreHardwareMonitor",
            @"\\.\root\OpenHardwareMonitor"
        };

        private readonly ObjectQuery SensorQuery = new ObjectQuery("SELECT * FROM Sensor");
        private readonly ObjectQuery HardwareQuery = new ObjectQuery("SELECT * FROM Hardware");

        private ManagementScope HardwareMonitorScope;
        private ManagementObjectSearcher SensorSearcher;
        private ManagementObjectSearcher HardwareSearcher;

        private readonly Dictionary<string, DynamicChild<SensorDynamicDataModel>> _cache = new();

        public override void Enable()
        {
            foreach (string scope in Scopes)
            {
                try
                {
                    HardwareMonitorScope = new ManagementScope(scope, null);
                    HardwareMonitorScope.Connect();
                }
                catch
                {
                    _logger.Warning($"Could not connect to WMI scope: {scope}");
                    //if the connection to one of the scopes fails,
                    //ignore the exception and try the other one.
                    //this way both Open and Libre HardwareMonitors
                    //can be supported since only the name of
                    //scope differs
                    continue;
                }

                SensorSearcher = new ManagementObjectSearcher(HardwareMonitorScope, SensorQuery);
                HardwareSearcher = new ManagementObjectSearcher(HardwareMonitorScope, HardwareQuery);

                List<Sensor> sensors = Sensor.FromCollection(SensorSearcher.Get());
                List<Hardware> hardwares = Hardware.FromCollection(HardwareSearcher.Get());

                if (sensors.Count == 0 || hardwares.Count == 0)
                {
                    _logger.Warning($"Connected to WMI scope \"{scope}\" but it did not contain any data.");
                    continue;
                }

                int hardwareIdCounter = 0;
                foreach (Hardware hw in hardwares.OrderBy(hw => hw.HardwareType))
                {
                    //loop through the hardware,
                    //and find all the sensors that hardware has
                    IEnumerable<Sensor> children = sensors.Where(s => s.Parent == hw.Identifier);

                    //if we don't find any sensors, skip and do the next hardware
                    if (!children.Any())
                        continue;

                    HardwareDynamicDataModel hwDataModel = DataModel.AddDynamicChild(
                        $"{hw.HardwareType}{hardwareIdCounter++}",
                        new HardwareDynamicDataModel(),
                        hw.Name,
                        hw.HardwareType.ToString()
                    ).Value;

                    //group sensors by type for easier UI navigation.
                    //this is also the way the UI of the HardwareMonitor
                    //programs displays the sensors, so let's keep that consistent
                    foreach (IGrouping<SensorType, Sensor> sensorsOfType in children.GroupBy(s => s.SensorType))
                    {
                        SensorTypeDynamicDataModel sensorTypeDataModel = hwDataModel.AddDynamicChild(
                            sensorsOfType.Key.ToString(),
                            new SensorTypeDynamicDataModel()
                        ).Value;

                        int sensorIdCounter = 0;
                        //for each type of sensor, we add all the sensors we found
                        foreach (Sensor sensorOfType in sensorsOfType.OrderBy(s => s.Name))
                        {
                            //this switch is only useful for the unit of each sensor
                            string unit = sensorsOfType.Key switch
                            {
                                SensorType.Temperature => "°C",
                                SensorType.Load => "%",
                                SensorType.Level => "%",
                                SensorType.Voltage => "V",
                                SensorType.SmallData => "MB",
                                SensorType.Data => "GB",
                                SensorType.Power => "W",
                                SensorType.Fan => "RPM",
                                SensorType.Throughput => "B/s",
                                SensorType.Clock => "MHz",
                                SensorType.Factor => "MHz",
                                _ => "",
                            };

                            DataModelPropertyAttribute attribute = new()
                            {
                                Affix = unit,
                                Name = sensorOfType.Name
                            };

                            DynamicChild<SensorDynamicDataModel> datamodel = sensorTypeDataModel.AddDynamicChild(
                                (sensorIdCounter++).ToString(),
                                new SensorDynamicDataModel(sensorOfType.Identifier),
                                attribute
                            );

                            _cache.Add(sensorOfType.Identifier, datamodel);
                        }
                    }
                }
                AddTimedUpdate(TimeSpan.FromMilliseconds(500), UpdateData);
                _logger.Information($"Successfully connected to WMI scope: {scope}");
                return;
                //success!
            }
            throw new ArtemisPluginException(Plugin, "Could not find hardware monitor WMI scope with data");
        }

        public override void Disable()
        {
            SensorSearcher?.Dispose();
            HardwareSearcher?.Dispose();
        }

        public override void Update(double deltaTime) { }

        private void UpdateData(double deltaTime)
        {
            Dictionary<string, Sensor> sensors = Sensor.GetDictionary(SensorSearcher.Get());
            foreach ((_, var sensor) in sensors)
            {
                if (_cache.TryGetValue(sensor.Identifier, out var dynamicChild))
                {
                    dynamicChild.Value.CurrentValue = sensor?.Value ?? -1;
                    dynamicChild.Value.Minimum = sensor?.Min ?? -1;
                    dynamicChild.Value.Maximum = sensor?.Max ?? -1;
                }
            }
        }
    }
}
