using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Serilog;
using System.Diagnostics;
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

        private double timeSinceLastUpdate;

        private readonly ObjectQuery SensorQuery = new ObjectQuery("SELECT * FROM Sensor");
        private readonly ObjectQuery HardwareQuery = new ObjectQuery("SELECT * FROM Hardware");

        private ManagementScope HardwareMonitorScope;
        private ManagementObjectSearcher SensorSearcher;
        private ManagementObjectSearcher HardwareSearcher;

        public override void EnablePlugin()
        {
            foreach (var scope in Scopes)
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

                var sensors = Sensor.FromCollection(SensorSearcher.Get());
                var hardwares = Hardware.FromCollection(HardwareSearcher.Get());

                if (sensors.Count == 0 || hardwares.Count == 0)
                {
                    _logger.Warning($"Connected to WMI scope \"{scope}\" but it did not contain any data.");
                    continue;
                }

                int hardwareIdCounter = 0;
                foreach (var hw in hardwares.OrderBy(hw => hw.HardwareType))
                {
                    //loop through the hardware,
                    //and find all the sensors that hardware has
                    var children = sensors.Where(s => s.Parent == hw.Identifier);

                    //if we don't find any sensors, skip and do the next hardware
                    if (!children.Any())
                        continue;

                    var hwDataModel = DataModel.AddDynamicChild(
                        new HardwareDynamicDataModel(),
                        $"{hw.HardwareType}{hardwareIdCounter++}",
                        hw.Name,
                        hw.HardwareType.ToString()
                    );

                    //group sensors by type for easier UI navigation.
                    //this is also the way the UI of the HardwareMonitor
                    //programs displays the sensors, so let's keep that consistent
                    foreach (var sensorsOfType in children.GroupBy(s => s.SensorType))
                    {
                        var sensorTypeDataModel = hwDataModel.AddDynamicChild(
                            new SensorTypeDynamicDataModel(),
                            sensorsOfType.Key.ToString()
                        );

                        int sensorIdCounter = 0;
                        //for each type of sensor, we add all the sensors we found
                        foreach (var sensorOfType in sensorsOfType.OrderBy(s => s.Name))
                        {
                            //this switch is only useful for the unit of each sensor
                            var dataModel = sensorsOfType.Key switch
                            {
                                SensorType.Temperature => new TemperatureDynamicDataModel(sensorOfType.Identifier),
                                SensorType.Load        => new PercentageDynamicDataModel (sensorOfType.Identifier),
                                SensorType.Level       => new PercentageDynamicDataModel (sensorOfType.Identifier),
                                SensorType.Voltage     => new VoltageDynamicDataModel    (sensorOfType.Identifier),
                                SensorType.SmallData   => new SmallDataDynamicDataModel  (sensorOfType.Identifier),
                                SensorType.Data        => new BigDataDynamicDataModel    (sensorOfType.Identifier),
                                SensorType.Power       => new PowerDynamicDataModel      (sensorOfType.Identifier),
                                SensorType.Fan         => new FanDynamicDataModel        (sensorOfType.Identifier),
                                SensorType.Throughput  => new ThroughputDynamicDataModel (sensorOfType.Identifier),
                                SensorType.Clock       => new ClockDynamicDataModel      (sensorOfType.Identifier),
                                _                      => new SensorDynamicDataModel     (sensorOfType.Identifier),
                            };

                            sensorTypeDataModel.AddDynamicChild(
                                dataModel,
                                (sensorIdCounter++).ToString(),
                                sensorOfType.Name
                            );
                        }
                    }
                }

                _logger.Information($"Successfully connected to WMI scope: {scope}");
                return;
                //success!
            }
            throw new ArtemisPluginException(PluginInfo, "Could not find hardware monitor WMI scope with data");
        }

        public override void DisablePlugin()
        {
            SensorSearcher?.Dispose();
            HardwareSearcher?.Dispose();
        }

        public override void Update(double deltaTime)
        {
            //update every second for now
            if (timeSinceLastUpdate < 1d)
            {
                timeSinceLastUpdate += deltaTime;
                return;
            }
            else
            {
                timeSinceLastUpdate = 0;
            }

            var sensors = Sensor.GetDictionary(SensorSearcher.Get());
            foreach (var (hardwareId, hardwareDataModel) in DataModel.DynamicDataModels)
            {
                foreach (var (sensorTypeId, sensorTypeDataModel) in hardwareDataModel.DynamicDataModels)
                {
                    foreach (var (sensorId, sensorDataModel) in sensorTypeDataModel.DynamicDataModels)
                    {
                        if (sensorDataModel is SensorDynamicDataModel s)
                        {
                            if (sensors.TryGetValue(s.Identifier, out var sensor))
                            {
                                s.CurrentValue = sensor?.Value ?? -1;
                                s.Minimum = sensor?.Min ?? -1;
                                s.Maximum = sensor?.Max ?? -1;
                            }
                        }
                    }
                }
            }
        }
    }
}
