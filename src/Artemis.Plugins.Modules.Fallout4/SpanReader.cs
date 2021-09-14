using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.Modules.Fallout4
{
    internal ref struct SpanReader
    {
        private readonly Span<byte> data;
        public ReadOnlySpan<byte> Data => data;
        public int Offset { get; private set; }

        public SpanReader(Span<byte> d)
        {
            data = d;
            Offset = 0;
        }

        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        public sbyte ReadSByte()
        {
            return unchecked((sbyte)ReadByte());
        }

        public byte ReadByte()
        {
            if (Offset + sizeof(byte) > data.Length)
                throw new Exception();

            var ret =  data[Offset];
            Offset += sizeof(byte);
            return ret;
        }

        public ushort ReadUInt16()
        {
            if (Offset + sizeof(ushort) > data.Length)
                throw new Exception();

            var ret = BinaryPrimitives.ReadUInt16LittleEndian(data[Offset..]);
            Offset += sizeof(ushort);
            return ret;
        }

        public int ReadInt32()
        {
            if (Offset + sizeof(int) > data.Length)
                throw new Exception();

            var ret = BinaryPrimitives.ReadInt32LittleEndian(data[Offset..]);
            Offset += sizeof(int);
            return ret;
        }

        public uint ReadUInt32()
        {
            if (Offset + sizeof(uint) > data.Length)
                throw new Exception();

            var ret = BinaryPrimitives.ReadUInt32LittleEndian(data[Offset..]);
            Offset += sizeof(uint);
            return ret;
        }

        public float ReadSingle()
        {
            if (Offset + sizeof(float) > data.Length)
                throw new Exception();

            var ret = BinaryPrimitives.ReadSingleLittleEndian(data[Offset..]);
            Offset += sizeof(float);
            return ret;
        }

        public string ReadNullTerminatedString()
        {
            StringBuilder builder = new();
            char c;

            while ((c = (char)ReadByte()) != char.MinValue)
                builder.Append(c);

            return builder.ToString();
        }

        public uint[] ReadArray()
        {
            var arraySize = ReadUInt16();
            var array = new uint[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                array[i] = ReadUInt32();
            }

            return array;
        }

        public SortedDictionary<uint, string> ReadMap()
        {
            SortedDictionary<uint, string> dict = new();

            var dictSize = ReadUInt16();
            for (int i = 0; i < dictSize; i++)
            {
                var key = ReadUInt32();
                var value = ReadNullTerminatedString();

                dict[key] = value;
            }
            return dict;
        }
    }
}
