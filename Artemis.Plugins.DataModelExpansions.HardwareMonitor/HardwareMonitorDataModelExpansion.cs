using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Serilog;
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
                    //if we can't connect to one of the scopes,
                    //ignore the exception and try the rest
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

                //first we find the availale hardwares
                foreach (var hw in hardwares)
                {
                    var children = sensors.Where(s => s.Parent == hw.Identifier).ToList();
                    if (children.Count == 0)
                        continue;
                    //we add a hardware data model to the main datamodel if it has sensors
                    var hwDataModel = DataModel.AddDynamicChild(new HardwareDynamicDataModel(), hw.Identifier, hw.Name, hw.HardwareType);

                    //then, for each hardware we add a data model for reach sensor type (temperature, load, etc.)
                    foreach (var sensorsOfType in children.GroupBy(s => s.SensorType))
                    {
                        var sensorTypeDataModel = hwDataModel.AddDynamicChild(
                            new SensorTypeDynamicDataModel(),
                            $"{hw.Identifier}/{sensorsOfType.Key}",
                            sensorsOfType.Key.ToString());

                        //for each type of sensor, we add all the sensors we found
                        foreach (var sensorOfType in sensorsOfType)
                        {
                            //this switch is only useful for the unit of each sensor
                            var dataModel = sensorsOfType.Key switch
                            {
                                SensorType.Temperature => new TemperatureDynamicDataModel(),
                                SensorType.Load => new PercentageDynamicDataModel(),
                                SensorType.Level => new PercentageDynamicDataModel(),
                                SensorType.Voltage => new VoltageDynamicDataModel(),
                                SensorType.SmallData => new SmallDataDynamicDataModel(),
                                SensorType.Data => new BigDataDynamicDataModel(),
                                SensorType.Power => new PowerDynamicDataModel(),
                                SensorType.Fan => new FanDynamicDataModel(),
                                SensorType.Throughput => new ThroughputDynamicDataModel(),
                                SensorType.Clock => new ClockDynamicDataModel(),
                                _ => new SensorDynamicDataModel(),
                            };
                            sensorTypeDataModel.AddDynamicChild(dataModel, sensorOfType.Identifier, sensorOfType.Name);
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

            var sensors = Sensor.FromCollection(SensorSearcher.Get());
            foreach (var (hardwareId, hardwareDataModel) in DataModel.DynamicDataModels)
            {
                foreach (var (sensorTypeId, sensorTypeDataModel) in hardwareDataModel.DynamicDataModels)
                {
                    foreach (var (sensorId, sensorDataModel) in sensorTypeDataModel.DynamicDataModels)
                    {
                        if (sensorDataModel is SensorDynamicDataModel s)
                        {
                            var sensor = sensors.Find(s => s.Identifier == sensorId);
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
