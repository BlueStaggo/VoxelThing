using System.Numerics;

namespace PDS;

public class ArrayItem<T>(T[] value) : StructureItem<T[]>(value)
    where T : IBinaryInteger<T>
{
    private static readonly int BytesPerItem = int.CreateChecked(T.PopCount(T.AllBitsSet)) / 8;

    public override sbyte[] SByteArrayValue => typeof(T) != typeof(sbyte) ? base.SByteArrayValue : (sbyte[])(Array)Value;
    public override byte[] ByteArrayValue => typeof(T) != typeof(byte) ? base.ByteArrayValue : (byte[])(Array)Value;
    public override short[] ShortArrayValue => typeof(T) != typeof(short) ? base.ShortArrayValue : (short[])(Array)Value;
    public override ushort[] UShortArrayValue => typeof(T) != typeof(ushort) ? base.UShortArrayValue : (ushort[])(Array)Value;
    public override int[] IntArrayValue => typeof(T) != typeof(int) ? base.IntArrayValue : (int[])(Array)Value;
    public override uint[] UIntArrayValue => typeof(T) != typeof(uint) ? base.UIntArrayValue : (uint[])(Array)Value;
    public override long[] LongArrayValue => typeof(T) != typeof(long) ? base.LongArrayValue : (long[])(Array)Value;
    public override ulong[] ULongArrayValue => typeof(T) != typeof(ulong) ? base.ULongArrayValue : (ulong[])(Array)Value;

    public ArrayItem() : this([]) { }
    
    protected override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        if (length <= 0)
        {
            Value = [];
            return;
        }

        byte[] bytes = reader.ReadBytes(length);
        if (typeof(T[]) == typeof(byte[]) || typeof(T[]) == typeof(sbyte[]))
        {
            Value = (T[])(Array)bytes;
            return;
        }

        int newLength = length / BytesPerItem;
        var array = new T[newLength];
        Buffer.BlockCopy(bytes, 0, array, 0, length);
        Value = array;
    }

    protected override void Write(BinaryWriter writer)
    {
        int length = Value.Length * BytesPerItem;
        writer.Write(length);

        if (length <= 0) return;

        byte[] bytes;
        if (typeof(T[]) == typeof(byte[]) || typeof(T[]) == typeof(sbyte[]))
        {
            bytes = (byte[])(Array)Value;
        }
        else
        {
            bytes = new byte[length];
            Buffer.BlockCopy(Value, 0, bytes, 0, length);
        }
        writer.Write(bytes);
    }
}
