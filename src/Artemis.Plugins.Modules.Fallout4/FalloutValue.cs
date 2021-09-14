using Artemis.Plugins.Modules.Fallout4.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace Artemis.Plugins.Modules.Fallout4
{
    public abstract class FalloutValue
    {
        public abstract FalloutDataType DataType { get; }
    }

    [DebuggerDisplay("{DataType} - {Data}")]
    public abstract class FalloutValue<T> : FalloutValue
    {
        public T Data { get; }

        protected FalloutValue(T data)
        {
            Data = data;
        }
    }

    public class BoolFalloutElement : FalloutValue<bool>
    {
        public override FalloutDataType DataType => FalloutDataType.Boolean;

        public BoolFalloutElement(bool data) : base(data) { }
    }

    public class SByteFalloutElement : FalloutValue<sbyte>
    {
        public override FalloutDataType DataType => FalloutDataType.SByte;

        public SByteFalloutElement(sbyte data) : base(data) { }
    }

    public class ByteFalloutElement : FalloutValue<byte>
    {
        public override FalloutDataType DataType => FalloutDataType.Byte;

        public ByteFalloutElement(byte data) : base(data) { }
    }

    public class IntFalloutElement : FalloutValue<int>
    {
        public override FalloutDataType DataType => FalloutDataType.Int;

        public IntFalloutElement(int data) : base(data) { }
    }

    public class UIntFalloutElement : FalloutValue<uint>
    {
        public override FalloutDataType DataType => FalloutDataType.UInt;

        public UIntFalloutElement(uint data) : base(data) { }
    }

    public class FloatFalloutElement : FalloutValue<float>
    {
        public override FalloutDataType DataType => FalloutDataType.Float;

        public FloatFalloutElement(float data) : base(data) { }
    }

    public class StringFalloutElement : FalloutValue<string>
    {
        public override FalloutDataType DataType => FalloutDataType.String;

        public StringFalloutElement(string data) : base(data) { }
    }

    public class ArrayFalloutElement : FalloutValue<uint[]>
    {
        public override FalloutDataType DataType => FalloutDataType.Array;

        public ArrayFalloutElement(uint[] data) : base(data) { }
    }

    public class MapFalloutElement : FalloutValue<IDictionary<uint, string>>
    {
        public override FalloutDataType DataType => FalloutDataType.Map;

        public MapFalloutElement(IDictionary<uint, string> dict) : base(dict) { }
    }
}
