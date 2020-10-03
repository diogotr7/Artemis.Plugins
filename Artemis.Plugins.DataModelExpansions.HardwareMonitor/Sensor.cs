using System;
using System.Collections.Generic;
using System.Management;

namespace Artemis.Plugins.DataModelExpansions.HardwareMonitor
{
    public class Sensor : IComparable
    {
        public string InstanceId { get; set; }
        public string ProcessId { get; set; }
        public string Identifier { get; set; }
        public int Index { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float Value { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public string SensorType { get; set; }

        public Sensor(ManagementBaseObject obj)
        {
            InstanceId = (string)obj["InstanceId"];
            ProcessId = (string)obj["ProcessId"];
            Identifier = (string)obj["Identifier"];
            Index = (int)obj["Index"];
            Min = (float)obj["Min"];
            Max = (float)obj["Max"];
            Value = (float)obj["Value"];
            Name = (string)obj["Name"];
            Parent = (string)obj["Parent"];
            SensorType = (string)obj["SensorType"];
        }

        public static List<Sensor> FromCollection(ManagementObjectCollection collection)
        {
            var list = new List<Sensor>(collection.Count);

            foreach (var obj in collection)
            {
                list.Add(new Sensor(obj));
            }
                
            return list;
        }

        public override string ToString() => $"{Identifier} : {Value}";

        public int CompareTo(object other)
        {
            return Identifier.CompareTo(((Sensor)other).Identifier);
        }
    }
}
