using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using System;
using System.Security.Principal;
using LibreHardwareMonitor.Hardware;
using System.Linq;

namespace Artemis.Plugins.DataModelExpansions.StandaloneHardwareMonitor
{
    public class HardwareMonitorDataModelExpansion : DataModelExpansion<HardwareMonitorDataModel>
    {
        private Computer _computer;
        private readonly UpdateVisitor _visitor = new UpdateVisitor();

        public override void Enable()
        {
            if (!IsUserAdministrator())
                throw new ArtemisPluginException("Admin privileges required");

            _computer = new Computer
            {
                IsControllerEnabled = true,
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true
            };

            _computer.Open();

            UpdateDynamicDataModels();
            AddTimedUpdate(TimeSpan.FromSeconds(1), UpdateHarware);
        }

        public override void Disable()
        {
            _computer?.Close();
        }

        public override void Update(double deltaTime)
        {
        }

        private void UpdateDynamicDataModels()
        {
            int hardwareIdCounter = 0;
            var allOfThem = _computer.Hardware.Concat(_computer.Hardware.SelectMany(h => h.SubHardware));
            foreach (IHardware hw in allOfThem.OrderBy(hw => hw.HardwareType))
            {
                if (!hw.Sensors.Any())
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
                foreach (var sensorsOfType in hw.Sensors.GroupBy(s => s.SensorType))
                {
                    SensorTypeDynamicDataModel sensorTypeDataModel = hwDataModel.AddDynamicChild(
                        sensorsOfType.Key.ToString(),
                        new SensorTypeDynamicDataModel()
                    ).Value;

                    int sensorIdCounter = 0;
                    //for each type of sensor, we add all the sensors we found
                    foreach (var sensorOfType in sensorsOfType.OrderBy(s => s.Name))
                    {
                        SensorDynamicDataModel dataModel = sensorsOfType.Key switch
                        {
                            SensorType.Temperature => new TemperatureDynamicDataModel(sensorOfType),
                            SensorType.Load => new PercentageDynamicDataModel(sensorOfType),
                            SensorType.Level => new PercentageDynamicDataModel(sensorOfType),
                            SensorType.Voltage => new VoltageDynamicDataModel(sensorOfType),
                            SensorType.SmallData => new SmallDataDynamicDataModel(sensorOfType),
                            SensorType.Data => new BigDataDynamicDataModel(sensorOfType),
                            SensorType.Power => new PowerDynamicDataModel(sensorOfType),
                            SensorType.Fan => new FanDynamicDataModel(sensorOfType),
                            SensorType.Throughput => new ThroughputDynamicDataModel(sensorOfType),
                            SensorType.Clock => new ClockDynamicDataModel(sensorOfType),
                            _ => new SensorDynamicDataModel(sensorOfType),
                        };

                        sensorTypeDataModel.AddDynamicChild(
                            (sensorIdCounter++).ToString(),
                            dataModel,
                            sensorOfType.Name
                        );
                    }
                }
            }
        }

        private void UpdateHarware(double deltaTime)
        {
            Profiler.StartMeasurement(nameof(UpdateHarware));

            _computer.Accept(_visitor);

            Profiler.StopMeasurement(nameof(UpdateHarware));
        }

        private static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            WindowsIdentity user = null;
            try
            {
                //get the currently logged in user
                user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            finally
            {
                if (user != null)
                    user.Dispose();
            }
            return isAdmin;
        }
    }
}