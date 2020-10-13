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
        public SensorType SensorType { get; set; }

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
            SensorType = Enum.Parse<SensorType>((string)obj["SensorType"]);
        }

        public static List<Sensor> FromCollection(ManagementObjectCollection collection)
        {
            var list = new List<Sensor>(collection.Count);

            foreach (var obj in collection)
            {
                var sensor = new Sensor(obj);
                if (sensor.SensorType != SensorType.Control)
                {
                    list.Add(sensor);
                }
            }

            return list;
        }

        public static Dictionary<string, Sensor> GetDictionary(ManagementObjectCollection collection)
        {
            var dict = new Dictionary<string, Sensor>();

            foreach (var obj in collection)
            {
                var sensor = new Sensor(obj);
                if (sensor.SensorType != SensorType.Control)
                {
                    dict.Add(sensor.Identifier, sensor);
                }
            }

            return dict;
        }

        public override string ToString() => $"{Identifier} : {Value}";

        public int CompareTo(object other)
        {
            return Identifier.CompareTo(((Sensor)other).Identifier);
        }
    }

    public enum SensorType
    {
        Temperature,
        Voltage,
        Level,
        SmallData,
        Load,
        Data,
        Power,
        Fan,
        Throughput,
        Factor,
        Control,
        Clock
    }
}
