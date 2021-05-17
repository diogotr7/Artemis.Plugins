using Artemis.Core.DataModelExpansions;
using Artemis.Plugins.DataModelExpansions.HWiNFO64.DataModels;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace Artemis.Plugins.DataModelExpansions.HWiNFO64
{
    public class HwInfoDataModelExpansion : DataModelExpansion<HwInfoDataModel>
    {
        private const string SHARED_MEMORY = @"Global\HWiNFO_SENS_SM2";
        private static readonly int sizeOfHwInfoRoot = Marshal.SizeOf<HwInfoRoot>();
        private static readonly int sizeOfHwInfoHardware = Marshal.SizeOf<HwInfoHardware>();
        private static readonly int sizeOfHwInfoSensor = Marshal.SizeOf<HwInfoSensor>();

        private MemoryMappedFile _memoryMappedFile;
        private MemoryMappedViewStream _rootStream;
        private MemoryMappedViewStream _hardwareStream;
        private MemoryMappedViewStream _sensorStream;

        private HwInfoRoot _hwInfoRoot;
        private HwInfoHardware[] _hardwares;
        private HwInfoSensor[] _sensors;

        private readonly Dictionary<ulong, DynamicChild<SensorDynamicDataModel>> _cache = new();

        private readonly byte[] _hardwareBuffer = new byte[sizeOfHwInfoHardware];
        private readonly byte[] _sensorBuffer = new byte[sizeOfHwInfoSensor];

        public override void Enable()
        {
            try
            {
                _memoryMappedFile = MemoryMappedFile.OpenExisting(SHARED_MEMORY, MemoryMappedFileRights.Read);

                _rootStream = _memoryMappedFile.CreateViewStream(
                    0,
                    sizeOfHwInfoRoot,
                    MemoryMappedFileAccess.Read);

                var hwinfoRootBytes = new byte[sizeOfHwInfoRoot];
                _rootStream.Read(hwinfoRootBytes, 0, sizeOfHwInfoRoot);
                _hwInfoRoot = BytesToStruct<HwInfoRoot>(hwinfoRootBytes);

                _hardwareStream = _memoryMappedFile.CreateViewStream(
                    _hwInfoRoot.HardwareSectionOffset,
                    _hwInfoRoot.HardwareCount * sizeOfHwInfoHardware,
                    MemoryMappedFileAccess.Read);

                _sensorStream = _memoryMappedFile.CreateViewStream(
                    _hwInfoRoot.SensorSectionOffset,
                    _hwInfoRoot.SensorCount * sizeOfHwInfoSensor,
                    MemoryMappedFileAccess.Read);
            }
            catch
            {
                //log failure, etc
                throw;
            }

            _hardwares = new HwInfoHardware[_hwInfoRoot.HardwareCount];
            _sensors = new HwInfoSensor[_hwInfoRoot.SensorCount];

            UpdateHardwares();

            UpdateSensors();

            PopulateDynamicDataModels();

            AddTimedUpdate(TimeSpan.FromMilliseconds(_hwInfoRoot.PollingPeriod), UpdateSensorsAndDataModel, nameof(UpdateSensorsAndDataModel));
        }

        public override void Disable()
        {
            _memoryMappedFile?.Dispose();
            _rootStream?.Dispose();
            _hardwareStream?.Dispose();
            _sensorStream?.Dispose();
            _cache.Clear();
            _hwInfoRoot = default;
            _hardwares = null;
            _sensors = null;
        }

        public override void Update(double deltaTime)
        {
            //updates are done only as often as HWiNFO64 polls to save resources.
        }

        private void UpdateSensorsAndDataModel(double deltaTime)
        {
            UpdateSensors();

            foreach (var item in _sensors)
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
            for (int i = 0; i < _hardwares.Length; i++)
            {
                HwInfoHardware hw = _hardwares[i];

                IEnumerable<HwInfoSensor> children = _sensors.Where(re => re.ParentIndex == i);

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
            _hardwareStream.Seek(0, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < _hwInfoRoot.HardwareCount; i++)
            {
                _hardwareStream.Read(_hardwareBuffer, 0, sizeOfHwInfoHardware);

                _hardwares[i] = BytesToStruct<HwInfoHardware>(_hardwareBuffer);
            }
        }

        private void UpdateSensors()
        {
            _sensorStream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < _hwInfoRoot.SensorCount; i++)
            {
                _sensorStream.Read(_sensorBuffer, 0, sizeOfHwInfoSensor);

                _sensors[i] = BytesToStruct<HwInfoSensor>(_sensorBuffer);
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