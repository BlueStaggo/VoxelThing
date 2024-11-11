using System.Numerics;

namespace PDS;

public class NumericItem<T>(T value) : StructureItem<T>(value)
    where T : INumberBase<T>
{
    private static readonly ReadMethod<T> NumericReadMethod = GetNumericReadMethod();
    private static readonly WriteMethod<T> NumericWriteMethod = GetNumericWriteMethod();

    public override sbyte SByteValue => sbyte.CreateChecked(Value);
    public override byte ByteValue => byte.CreateChecked(Value);
    public override short ShortValue => short.CreateChecked(Value);
    public override ushort UShortValue => ushort.CreateChecked(Value);
    public override int IntValue => int.CreateChecked(Value);
    public override uint UIntValue => uint.CreateChecked(Value);
    public override long LongValue => long.CreateChecked(Value);
    public override ulong ULongValue => ulong.CreateChecked(Value);
    public override bool BoolValue => !T.IsZero(Value);
    public override float FloatValue => float.CreateChecked(Value);
    public override double DoubleValue => double.CreateChecked(Value);

    public NumericItem() : this(T.Zero) { }

    protected override void Read(BinaryReader reader)
        => Value = NumericReadMethod.Invoke(reader);

    protected override void Write(BinaryWriter writer)
        => NumericWriteMethod.Invoke(writer, Value);

    private static ReadMethod<T> GetNumericReadMethod()
    {
        if (typeof(T) == typeof(sbyte)) return reader => T.CreateTruncating(reader.ReadSByte());
        if (typeof(T) == typeof(byte)) return reader => T.CreateTruncating(reader.ReadByte());
        if (typeof(T) == typeof(short)) return reader => T.CreateTruncating(reader.ReadInt16());
        if (typeof(T) == typeof(ushort)) return reader => T.CreateTruncating(reader.ReadUInt16());
        if (typeof(T) == typeof(int)) return reader => T.CreateTruncating(reader.ReadInt32());
        if (typeof(T) == typeof(uint)) return reader => T.CreateTruncating(reader.ReadUInt32());
        if (typeof(T) == typeof(long)) return reader => T.CreateTruncating(reader.ReadInt64());
        if (typeof(T) == typeof(ulong)) return reader => T.CreateTruncating(reader.ReadUInt64());
        if (typeof(T) == typeof(float)) return reader => T.CreateTruncating(reader.ReadSingle());
        if (typeof(T) == typeof(double)) return reader => T.CreateTruncating(reader.ReadDouble());

        throw new InvalidOperationException("No read method available");
    }

    private static WriteMethod<T> GetNumericWriteMethod()
    {
        if (typeof(T) == typeof(sbyte)) return (writer, value) => writer.Write(sbyte.CreateChecked(value));
        if (typeof(T) == typeof(byte)) return (writer, value) => writer.Write(byte.CreateChecked(value));
        if (typeof(T) == typeof(short)) return (writer, value) => writer.Write(short.CreateChecked(value));
        if (typeof(T) == typeof(ushort)) return (writer, value) => writer.Write(ushort.CreateChecked(value));
        if (typeof(T) == typeof(int)) return (writer, value) => writer.Write(int.CreateChecked(value));
        if (typeof(T) == typeof(uint)) return (writer, value) => writer.Write(uint.CreateChecked(value));
        if (typeof(T) == typeof(long)) return (writer, value) => writer.Write(long.CreateChecked(value));
        if (typeof(T) == typeof(ulong)) return (writer, value) => writer.Write(ulong.CreateChecked(value));
        if (typeof(T) == typeof(float)) return (writer, value) => writer.Write(float.CreateChecked(value));
        if (typeof(T) == typeof(double)) return (writer, value) => writer.Write(double.CreateChecked(value));

        throw new InvalidOperationException("No write method available");
    }

    private delegate TU ReadMethod<out TU>(BinaryReader reader);

    private delegate void WriteMethod<in TU>(BinaryWriter writer, TU value);
}