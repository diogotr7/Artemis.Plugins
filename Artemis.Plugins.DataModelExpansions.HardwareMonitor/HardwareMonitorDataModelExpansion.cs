using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Serilog;
using System;
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

        private ManagementObjectSearcher SensorSearcher;
        private ManagementScope HardwareMonitorScope;
        private readonly ObjectQuery SensorQuery
            = new ObjectQuery("SELECT * FROM Sensor WHERE " +
                          "(Identifier LIKE \"%cpu%\" " +
                        "OR Identifier LIKE \"%gpu%\" " +
                        "OR Identifier LIKE \"%ram%\")");

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
                if (SensorSearcher.Get().Count != 0)
                {
                    _logger.Information($"Successfully connected to WMI scope: {scope}");
                    return;//success
                }
            }
            throw new ArtemisPluginException(PluginInfo, "Could not find hardware monitor WMI scope with data");
        }

        public override void DisablePlugin()
        {
            SensorSearcher?.Dispose();
        }

        public override void Update(double deltaTime)
        {
            //update every half a second for now
            if (timeSinceLastUpdate < 0.50d)
            {
                timeSinceLastUpdate += deltaTime;
                return;
            }
            else
            {
                timeSinceLastUpdate = 0;
            }

            try
            {
                var sensors = Sensor.FromCollection(SensorSearcher.Get());

                sensors.Sort();

                var gpuSensors = sensors.Where(s => s.Identifier.Contains("gpu"));
                var cpuSensors = sensors.Where(s => s.Identifier.Contains("cpu"));
                var ramSensors = sensors.Where(s => s.Identifier.Contains("ram"));

                var gpuUsage = gpuSensors.FirstOrDefault(sns => sns.SensorType == "Load");
                var gpuTemp = gpuSensors.FirstOrDefault(sns => sns.SensorType == "Temperature");
                var gpuPower = gpuSensors.FirstOrDefault(sns => sns.SensorType == "Power");

                var cpuUsage = cpuSensors.FirstOrDefault(sns => sns.SensorType == "Load");
                var cpuTemp = cpuSensors.FirstOrDefault(sns => sns.SensorType == "Temperature");
                var cpuPower = cpuSensors.FirstOrDefault(sns => sns.SensorType == "Power");

                var ramUsed = ramSensors.FirstOrDefault(sns => sns.Name.Contains("Used"));
                var ramTotal = ramSensors.FirstOrDefault(sns => sns.Name == "Memory");

                DataModel.Cpu.Usage = cpuUsage?.Value ?? -1;
                DataModel.Cpu.Temperature = cpuTemp?.Value ?? -1;
                DataModel.Cpu.PowerUsage = cpuPower?.Value ?? -1;
                DataModel.Gpu.Usage = gpuUsage?.Value ?? -1;
                DataModel.Gpu.Temperature = gpuTemp?.Value ?? -1;
                DataModel.Gpu.PowerUsage = gpuPower?.Value ?? -1;
                DataModel.RamUsed = ramUsed?.Value ?? -1;
                DataModel.RamTotal = ramTotal?.Value ?? -1;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
        }
    }
}
