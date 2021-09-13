using Artemis.Plugins.Modules.Fallout4.Enums;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4
{
    internal interface INode
    {
        uint Id { get; }
        FalloutDataType DataType { get; set; }
        void Fill(IDictionary<uint, (FalloutDataType DataType, object Data)> dict);
        object GetData();
    }

    internal class Node<T> : INode
    {
        public uint Id { get; }
        public FalloutDataType DataType { get; set; }
        public T Data { get; set; }

        protected Node(FalloutDataType dataType, uint id)
        {
            DataType = dataType;
            Id = id;
        }

        public virtual void Fill(IDictionary<uint, (FalloutDataType DataType, object Data)> dict)
        {
            Data = (T)dict[Id].Data;
        }

        protected static INode GetNodeType(FalloutDataType type, uint id) => type switch
        {
            FalloutDataType.Boolean => new Node<bool>(type, id),
            FalloutDataType.SByte => new Node<sbyte>(type, id),
            FalloutDataType.Byte => new Node<byte>(type, id),
            FalloutDataType.Int => new Node<int>(type, id),
            FalloutDataType.UInt => new Node<uint>(type, id),
            FalloutDataType.Float => new Node<float>(type, id),
            FalloutDataType.String => new Node<string>(type, id),
            FalloutDataType.Array => new ArrayNode(id),
            FalloutDataType.Map => new MapNode(id),
            _ => throw new NotImplementedException()
        };

        public override string ToString() => $"{DataType}: {Data}";

        public object GetData()
        {
            return Data;
        }
    }

    internal class ArrayNode : Node<INode[]>
    {
        public ArrayNode(uint id) : base(FalloutDataType.Array, id) { }

        public override void Fill(IDictionary<uint, (FalloutDataType DataType, object Data)> dict)
        {
            List<INode> list = new List<INode>();
            foreach (uint item in dict[Id].Data as uint[])
            {
                INode node = GetNodeType(dict[item].DataType, item);
                node.Fill(dict);
                list.Add(node);
            }
            Data = list.ToArray();
        }

        public override string ToString() => $"{DataType}: {Data.Length} elements";
    }

    internal class MapNode : Node<Dictionary<string, INode>>
    {
        public MapNode(uint id) : base(FalloutDataType.Map, id) { }

        public override void Fill(IDictionary<uint, (FalloutDataType DataType, object Data)> dict)
        {
            Data = new Dictionary<string, INode>();
            foreach (KeyValuePair<uint, string> item in dict[Id].Data as Dictionary<uint, string>)
            {
                INode node = GetNodeType(dict[item.Key].DataType, item.Key);
                node.Fill(dict);
                Data.Add(item.Value, node);
            }
        }

        public override string ToString() => $"{DataType}: {Data.Count} elements";
    }
}
