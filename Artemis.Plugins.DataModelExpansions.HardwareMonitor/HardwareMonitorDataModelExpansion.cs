using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows.Media.Animation;

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

        private ManagementScope HardwareMonitorScope;
        private ManagementObjectSearcher SensorSearcher;
        private ManagementObjectSearcher HardwareSearcher;

        private readonly ObjectQuery SensorQuery = new ObjectQuery("SELECT * FROM Sensor");
        private readonly ObjectQuery HardwareQuery = new ObjectQuery("SELECT * FROM Hardware");

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

                if (sensors.Count == 0)
                    break;

                //if we somehow connect to the scope
                //and it desnt have any sensors / hardwares,
                //what does this mean? TODO

                foreach (var hw in hardwares)
                {
                    var children = sensors.Where(s => s.Parent == hw.Identifier);
                    if (!children.Any())
                        continue;

                    DataModel.AddDynamicChild(new HardwareDynamicDataModel(), hw.Identifier, hw.Name, hw.HardwareType);
                    //if AddDynamicChild returned what it just added this would be simpler. 
                    //I have to fetch the dataModel i just added ¯\_(ツ)_/¯
                    var hwDataModel = DataModel.DynamicChild<HardwareDynamicDataModel>(hw.Identifier);

                    foreach (var sensor in children)
                    {
                        hwDataModel.AddDynamicChild(new SensorDynamicDataModel(), sensor.Identifier, sensor.Name, sensor.SensorType);
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
            foreach (var (hwId, hw) in DataModel.DynamicDataModels)
            {
                foreach (var (senId, sen) in hw.DynamicDataModels)
                {
                    if (sen is SensorDynamicDataModel sensorDynamicDataModel)
                    {
                        var sensor = sensors.Find(s => s.Identifier == senId);
                        sensorDynamicDataModel.SensorValue = sensor?.Value ?? 0;
                    }
                    else
                    {
                        throw new ArtemisPluginException(PluginInfo);//bad
                    }
                }
            }
        }
    }
}
