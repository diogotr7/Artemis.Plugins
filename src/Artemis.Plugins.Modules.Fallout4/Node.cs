using Artemis.Plugins.Modules.Fallout4.Enums;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Fallout4
{
    internal interface INode
    {
        uint Id { get; }
        DataType DataType { get; set; }
        void Fill(IDictionary<uint, (DataType DataType, object Data)> dict);
        object GetData();
    }

    internal class Node<T> : INode
    {
        public uint Id { get; }
        public DataType DataType { get; set; }
        public T Data { get; set; }

        protected Node(DataType dataType, uint id)
        {
            DataType = dataType;
            Id = id;
        }

        public virtual void Fill(IDictionary<uint, (DataType DataType, object Data)> dict)
        {
            Data = (T)dict[Id].Data;
        }

        protected static INode GetNodeType(DataType type, uint id) => type switch
        {
            DataType.Boolean => new Node<bool>(type, id),
            DataType.SByte => new Node<sbyte>(type, id),
            DataType.Byte => new Node<byte>(type, id),
            DataType.Int => new Node<int>(type, id),
            DataType.UInt => new Node<uint>(type, id),
            DataType.Float => new Node<float>(type, id),
            DataType.String => new Node<string>(type, id),
            DataType.Array => new ArrayNode(id),
            DataType.Map => new MapNode(id),
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
        public ArrayNode(uint id) : base(DataType.Array, id) { }

        public override void Fill(IDictionary<uint, (DataType DataType, object Data)> dict)
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
        public MapNode(uint id) : base(DataType.Map, id) { }

        public override void Fill(IDictionary<uint, (DataType DataType, object Data)> dict)
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
