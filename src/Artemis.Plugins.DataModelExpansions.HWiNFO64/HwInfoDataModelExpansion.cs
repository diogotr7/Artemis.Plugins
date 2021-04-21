using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.HWiNFO64.DataModels;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.DataModelExpansions.HWiNFO64
{
    public class HwInfoDataModelExpansion : DataModelExpansion<HwInfoDataModel>
    {
        private const string SHARED_MEMORY = @"Global\HWiNFO_SENS_SM2";
        private readonly int sizeOfHwInfoRoot = Marshal.SizeOf<HwInfoRoot>();
        private readonly int sizeOfHwInfoHardware = Marshal.SizeOf<HwInfoHardware>();
        private readonly int sizeOfHwInfoSensor = Marshal.SizeOf<HwInfoSensor>();

        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewStream rootStream;
        private MemoryMappedViewStream hardwareStream;
        private MemoryMappedViewStream sensorStream;

        private HwInfoRoot hwInfoRoot;
        private HwInfoHardware[] hardwares;
        private HwInfoSensor[] sensors;

        private readonly Dictionary<ulong, DynamicChild<SensorDynamicDataModel>> _cache = new();

        public override void Enable()
        {
            try
            {
                memoryMappedFile = MemoryMappedFile.OpenExisting(SHARED_MEMORY, MemoryMappedFileRights.Read);

                rootStream = memoryMappedFile.CreateViewStream(
                    0,
                    sizeOfHwInfoRoot,
                    MemoryMappedFileAccess.Read);

                var hwinfoRootBytes = new byte[sizeOfHwInfoRoot];
                rootStream.Read(hwinfoRootBytes, 0, sizeOfHwInfoRoot);
                hwInfoRoot = BytesToStruct<HwInfoRoot>(hwinfoRootBytes);

                hardwareStream = memoryMappedFile.CreateViewStream(
                    hwInfoRoot.HardwareSectionOffset,
                    hwInfoRoot.HardwareCount * sizeOfHwInfoHardware,
                    MemoryMappedFileAccess.Read);

                sensorStream = memoryMappedFile.CreateViewStream(
                    hwInfoRoot.SensorSectionOffset,
                    hwInfoRoot.SensorCount * sizeOfHwInfoSensor,
                    MemoryMappedFileAccess.Read);
            }
            catch
            {
                //log failure, etc
                throw;
            }

            hardwares = new HwInfoHardware[hwInfoRoot.HardwareCount];
            sensors = new HwInfoSensor[hwInfoRoot.SensorCount];

            UpdateHardwares();

            UpdateSensors();

            PopulateDynamicDataModels();

            AddTimedUpdate(TimeSpan.FromMilliseconds(hwInfoRoot.PollingPeriod), UpdataDataModelFromSensors);
        }

        public override void Disable()
        {
            memoryMappedFile?.Dispose();
            rootStream?.Dispose();
            hardwareStream?.Dispose();
            sensorStream?.Dispose();
            _cache.Clear();
            hwInfoRoot = default;
            hardwares = null;
            sensors = null;
        }

        public override void Update(double deltaTime)
        {
            //updates are done only as often as HWiNFO64 polls to save resources.
        }

        private void UpdataDataModelFromSensors(double deltaTime)
        {
            UpdateSensors();

            foreach (var item in sensors)
            {
                if (_cache.TryGetValue(item.Id, out var child))
                {
                    child.Value.CurrentValue = item.Value;
                    child.Value.Average = item.ValueAvg;
                    child.Value.Minimum = item.ValueMin;
                    child.Value.Maximum = item.ValueMax;
                }
            }
        }

        private void PopulateDynamicDataModels()
        {
            for (int i = 0; i < hardwares.Length; i++)
            {
                HwInfoHardware hw = hardwares[i];

                IEnumerable<HwInfoSensor> children = sensors.Where(re => re.ParentIndex == i);

                if (!children.Any())
                    continue;

                HardwareDynamicDataModel hardwareDataModel = DataModel.AddDynamicChild(
                    hw.GetId(),
                    new HardwareDynamicDataModel(),
                    hw.NameCustom,
                    hw.NameOriginal
                ).Value;

                foreach (IGrouping<HwInfoSensorType, HwInfoSensor> sensorsOfType in children.GroupBy(s => s.SensorType))
                {
                    SensorTypeDynamicDataModel sensorTypeDataModel = hardwareDataModel.AddDynamicChild(
                        sensorsOfType.Key.ToString(),
                        new SensorTypeDynamicDataModel()
                    ).Value;

                    //this will make it so the ids are something like:
                    //load1, load2, temperature1, temperature2
                    //which should be somewhat portable i guess
                    int sensorOfTypeIndex = 0;
                    foreach (HwInfoSensor sensor in sensorsOfType.OrderBy(s => s.LabelOriginal))
                    {
                        var dataModel = sensorTypeDataModel.AddDynamicChild(
                            $"{sensorsOfType.Key.ToString().ToLower()}{sensorOfTypeIndex++}",
                            new SensorDynamicDataModel(),
                            sensor.LabelCustom,
                            sensor.LabelOriginal
                        );

                        _cache.Add(sensor.Id, dataModel);
                    }
                }
            }
        }

        private void UpdateHardwares()
        {
            hardwareStream.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] bytes = new byte[sizeOfHwInfoHardware];

            for (int i = 0; i < hwInfoRoot.HardwareCount; i++)
            {
                hardwareStream.Read(bytes, 0, sizeOfHwInfoHardware);

                hardwares[i] = BytesToStruct<HwInfoHardware>(bytes);
            }
        }

        private void UpdateSensors()
        {
            sensorStream.Seek(0, System.IO.SeekOrigin.Begin);
            var bytes = new byte[sizeOfHwInfoSensor];

            for (int i = 0; i < hwInfoRoot.SensorCount; i++)
            {
                sensorStream.Read(bytes, 0, sizeOfHwInfoSensor);

                sensors[i] = BytesToStruct<HwInfoSensor>(bytes);
            }
        }

        private static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            T result;

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return result;
        }
    }
}