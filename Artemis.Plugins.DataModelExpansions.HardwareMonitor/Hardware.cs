using System;
using System.Collections.Generic;
using System.Management;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    class Hardware
    {
        public string InstanceId { get; set; }
        public string ProcessId { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public string HardwareType { get; set; }

        public Hardware(ManagementBaseObject obj)
        {
            InstanceId = (string)obj["InstanceId"];
            ProcessId = (string)obj["ProcessId"];
            Identifier = (string)obj["Identifier"];
            Name = (string)obj["Name"];
            Parent = (string)obj["Parent"];
            HardwareType = (string)obj["HardwareType"];
        }

        public static List<Hardware> FromCollection(ManagementObjectCollection collection)
        {
            var list = new List<Hardware>(collection.Count);

            foreach (var obj in collection)
                list.Add(new Hardware(obj));

            return list;
        }
    }
}
