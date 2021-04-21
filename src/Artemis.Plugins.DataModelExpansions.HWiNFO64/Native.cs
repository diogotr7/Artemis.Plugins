using System.Runtime.InteropServices;

namespace Artemis.Plugins.DataModelExpansions.HWiNFO64
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 68)]
    internal readonly struct HwInfoRoot
    {
        internal readonly uint Signature;
        internal readonly uint Version;
        internal readonly uint Revision;
        internal readonly long PollTime;

        internal readonly uint HardwareSectionOffset;
        internal readonly uint HardwareSize;
        internal readonly uint HardwareCount;

        internal readonly uint SensorSectionOffset;
        internal readonly uint SensorSize;
        internal readonly uint SensorCount;

        internal readonly uint PollingPeriod;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 264)]
    internal readonly struct HwInfoHardware
    {
        internal readonly uint HardwareId;

        internal readonly uint HardwareInstance;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal readonly string NameOriginal;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal readonly string NameCustom;

        internal string GetId() => $"{HardwareId}-{HardwareInstance}";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 316)]
    internal readonly struct HwInfoSensor
    {
        internal readonly HwInfoSensorType SensorType;

        internal readonly uint ParentIndex;

        internal readonly uint SensorId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal readonly string LabelOriginal;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        internal readonly string LabelCustom;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        internal readonly string SensorUnit;

        internal readonly double Value;

        internal readonly double ValueMin;

        internal readonly double ValueMax;

        internal readonly double ValueAvg;

        internal ulong Id => (ParentIndex * 100000000000ul) + SensorId;
    }

    public enum HwInfoSensorType
    {
        None = 0,
        Temperature,
        Volt,
        Fan,
        Current,
        Power,
        Clock,
        Usage,
        Other
    };
}