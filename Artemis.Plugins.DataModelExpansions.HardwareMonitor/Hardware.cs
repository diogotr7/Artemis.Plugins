using System;
using System.Collections.Generic;
using System.Management;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class Hardware
    {
        public string InstanceId { get; set; }
        public string ProcessId { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public HardwareType HardwareType { get; set; }

        public Hardware(ManagementBaseObject obj)
        {
            InstanceId = (string)obj["InstanceId"];
            ProcessId = (string)obj["ProcessId"];
            Identifier = (string)obj["Identifier"];
            Name = (string)obj["Name"];
            Parent = (string)obj["Parent"];
            HardwareType = GetHardwareType((string)obj["HardwareType"]);
        }

        public static List<Hardware> FromCollection(ManagementObjectCollection collection)
        {
            var list = new List<Hardware>(collection.Count);

            foreach (var obj in collection)
                list.Add(new Hardware(obj));

            return list;
        }

        private HardwareType GetHardwareType(string name)
        {
            return name.ToLower() switch
            {
                "motherboard" => HardwareType.Motherboard,
                "mainboard" => HardwareType.Motherboard,
                "superio" => HardwareType.SuperIO,
                "cpu" => HardwareType.Cpu,
                "memory" => HardwareType.Memory,
                "ram" => HardwareType.Memory,
                "gpunvidia" => HardwareType.Gpu,
                "gpuamd" => HardwareType.Gpu,
                "storage" => HardwareType.Storage,
                "hdd" => HardwareType.Storage,
                "network" => HardwareType.Network,
                "cooler" => HardwareType.Cooler,
                _ => HardwareType.Unknown
            };
        }
    }

    public enum HardwareType
    {
        Cpu,
        Gpu,
        Memory,
        Motherboard,
        SuperIO,
        Storage,
        Network,
        Cooler,
        Unknown
    }
}
