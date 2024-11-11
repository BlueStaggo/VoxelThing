using System.IO.Compression;
using System.Reflection;

namespace PDS;

public abstract class StructureItem
{
#region Type Maps
    public static readonly Type[] RegisteredTypes =
    [
        typeof(NumericItem<sbyte>),
        typeof(ArrayItem<sbyte>),
        typeof(NumericItem<byte>),
        typeof(ArrayItem<byte>),
        typeof(NumericItem<short>),
        typeof(ArrayItem<short>),
        typeof(NumericItem<ushort>),
        typeof(ArrayItem<ushort>),
        typeof(NumericItem<int>),
        typeof(ArrayItem<int>),
        typeof(NumericItem<uint>),
        typeof(ArrayItem<uint>),
        typeof(NumericItem<long>),
        typeof(ArrayItem<long>),
        typeof(NumericItem<ulong>),
        typeof(ArrayItem<ulong>),
        typeof(BoolItem),
        typeof(NumericItem<float>),
        typeof(NumericItem<double>),
        typeof(StringItem),
        typeof(CompoundItem),
        typeof(ListItem),
    ];

    public static readonly HashSet<Type> SupportedTypes =
    [
        typeof(sbyte),
        typeof(sbyte[]),
        typeof(byte),
        typeof(byte[]),
        typeof(short),
        typeof(short[]),
        typeof(ushort),
        typeof(ushort[]),
        typeof(int),
        typeof(int[]),
        typeof(uint),
        typeof(uint[]),
        typeof(long),
        typeof(long[]),
        typeof(ulong),
        typeof(ulong[]),
        typeof(bool),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(Dictionary<string, object?>),
        typeof(List<object?>),
    ];

    public static readonly Dictionary<Type, byte> RegisteredTypeIDs;

    private static readonly Dictionary<Type, ItemConstructor> SerializationTable = new()
    {
        [typeof(sbyte)] = value => new NumericItem<sbyte>((sbyte)value),
        [typeof(sbyte[])] = value => new ArrayItem<sbyte>((sbyte[])value),
        [typeof(byte)] = value => new NumericItem<byte>((byte)value),
        [typeof(byte[])] = value => new ArrayItem<byte>((byte[])value),
        [typeof(short)] = value => new NumericItem<short>((short)value),
        [typeof(short[])] = value => new ArrayItem<short>((short[])value),
        [typeof(ushort)] = value => new NumericItem<ushort>((ushort)value),
        [typeof(ushort[])] = value => new ArrayItem<ushort>((ushort[])value),
        [typeof(int)] = value => new NumericItem<int>((int)value),
        [typeof(int[])] = value => new ArrayItem<int>((int[])value),
        [typeof(uint)] = value => new NumericItem<uint>((uint)value),
        [typeof(uint[])] = value => new ArrayItem<uint>((uint[])value),
        [typeof(long)] = value => new NumericItem<long>((long)value),
        [typeof(long[])] = value => new ArrayItem<long>((long[])value),
        [typeof(ulong)] = value => new NumericItem<ulong>((ulong)value),
        [typeof(ulong[])] = value => new ArrayItem<ulong>((ulong[])value),
        [typeof(bool)] = value => new BoolItem((bool)value),
        [typeof(float)] = value => new NumericItem<float>((float)value),
        [typeof(double)] = value => new NumericItem<double>((double)value),
        [typeof(string)] = value => new StringItem((string)value),
    };

    private static readonly Dictionary<Type, ItemDeconstructor> DeserializationTable = new()
    {
        [typeof(NumericItem<sbyte>)] = value => value.SByteValue,
        [typeof(ArrayItem<sbyte>)] = value => value.SByteArrayValue,
        [typeof(NumericItem<byte>)] = value => value.ByteValue,
        [typeof(ArrayItem<byte>)] = value => value.ByteArrayValue,
        [typeof(NumericItem<short>)] = value => value.ShortValue,
        [typeof(ArrayItem<short>)] = value => value.ShortArrayValue,
        [typeof(NumericItem<ushort>)] = value => value.UShortValue,
        [typeof(ArrayItem<ushort>)] = value => value.UShortArrayValue,
        [typeof(NumericItem<int>)] = value => value.IntValue,
        [typeof(ArrayItem<int>)] = value => value.IntArrayValue,
        [typeof(NumericItem<uint>)] = value => value.UIntValue,
        [typeof(ArrayItem<uint>)] = value => value.UIntArrayValue,
        [typeof(NumericItem<long>)] = value => value.LongValue,
        [typeof(ArrayItem<long>)] = value => value.LongArrayValue,
        [typeof(NumericItem<ulong>)] = value => value.ULongValue,
        [typeof(ArrayItem<ulong>)] = value => value.ULongArrayValue,
        [typeof(BoolItem)] = value => value.BoolValue,
        [typeof(NumericItem<float>)] = value => value.ByteValue,
        [typeof(NumericItem<double>)] = value => value.ByteValue,
        [typeof(StringItem)] = value => value.ToString(),
    };
#endregion

#region Values
    public virtual string StringValue => ToString() ?? GetType().Name;

    public virtual sbyte SByteValue => ThrowInvalidValueType<sbyte>();
    public virtual sbyte[] SByteArrayValue => ThrowInvalidValueType<sbyte[]>();
    public virtual byte ByteValue => ThrowInvalidValueType<byte>();
    public virtual byte[] ByteArrayValue => ThrowInvalidValueType<byte[]>();

    public virtual short ShortValue => ThrowInvalidValueType<short>();
    public virtual short[] ShortArrayValue => ThrowInvalidValueType<short[]>();
    public virtual ushort UShortValue => ThrowInvalidValueType<ushort>();
    public virtual ushort[] UShortArrayValue => ThrowInvalidValueType<ushort[]>();

    public virtual int IntValue => ThrowInvalidValueType<int>();
    public virtual int[] IntArrayValue => ThrowInvalidValueType<int[]>();
    public virtual uint UIntValue => ThrowInvalidValueType<uint>();
    public virtual uint[] UIntArrayValue => ThrowInvalidValueType<uint[]>();

    public virtual long LongValue => ThrowInvalidValueType<long>();
    public virtual long[] LongArrayValue => ThrowInvalidValueType<long[]>();
    public virtual ulong ULongValue => ThrowInvalidValueType<ulong>();
    public virtual ulong[] ULongArrayValue => ThrowInvalidValueType<ulong[]>();

    public virtual bool BoolValue => ThrowInvalidValueType<bool>();
    public virtual float FloatValue => ThrowInvalidValueType<float>();
    public virtual double DoubleValue => ThrowInvalidValueType<double>();

    public virtual List<StructureItem> ListValue => ThrowInvalidValueType<List<StructureItem>>();
    public virtual Dictionary<string, StructureItem> DictionaryValue => ThrowInvalidValueType<Dictionary<string, StructureItem>>();
#endregion Values

#region Try Values (absolutely filthy code here please hide)
    // Dirty code in the back helps make clean code at the front
    public string? TryStringValue { get { try { return StringValue; } catch (Exception) { return null; } } }

    public sbyte? TrySByteValue { get { try { return SByteValue; } catch (Exception) { return null; } } }
    public sbyte[]? TrySByteArrayValue { get { try { return SByteArrayValue; } catch (Exception) { return null; } } }
    public byte? TryByteValue { get { try { return ByteValue; } catch (Exception) { return null; } } }
    public byte[]? TryByteArrayValue { get { try { return ByteArrayValue; } catch (Exception) { return null; } } }

    public short? TryShortValue { get { try { return ShortValue; } catch (Exception) { return null; } } }
    public short[]? TryShortArrayValue { get { try { return ShortArrayValue; } catch (Exception) { return null; } } }
    public ushort? TryUShortValue { get { try { return UShortValue; } catch (Exception) { return null; } } }
    public ushort[]? TryUShortArrayValue { get { try { return UShortArrayValue; } catch (Exception) { return null; } } }

    public int? TryIntValue { get { try { return IntValue; } catch (Exception) { return null; } } }
    public int[]? TryIntArrayValue { get { try { return IntArrayValue; } catch (Exception) { return null; } } }
    public uint? TryUIntValue { get { try { return UIntValue; } catch (Exception) { return null; } } }
    public uint[]? TryUIntArrayValue { get { try { return UIntArrayValue; } catch (Exception) { return null; } } }

    public long? TryLongValue { get { try { return LongValue; } catch (Exception) { return null; } } }
    public long[]? TryLongArrayValue { get { try { return LongArrayValue; } catch (Exception) { return null; } } }
    public ulong? TryULongValue { get { try { return ULongValue; } catch (Exception) { return null; } } }
    public ulong[]? TryULongArrayValue { get { try { return ULongArrayValue; } catch (Exception) { return null; } } }

    public bool? TryBooleanValue { get { try { return BoolValue; } catch (Exception) { return null; } } }
    public float? TryFloatValue { get { try { return FloatValue; } catch (Exception) { return null; } } }
    public double? TryDoubleValue { get { try { return DoubleValue; } catch (Exception) { return null; } } }

    public List<StructureItem>? TryListValue { get { try { return ListValue; } catch (Exception) { return null; } } }
    public Dictionary<string, StructureItem>? TryDictionaryValue { get { try { return DictionaryValue; } catch (Exception) { return null; } } }
#endregion Try Values (absolutely filthy code here please hide)

    private T ThrowInvalidValueType<T>() => throw new InvalidOperationException($"\"{GetType().Name}\" does not contain \"{typeof(T).Name}");

    public byte GetTypeId() => RegisteredTypeIDs[GetType()];

    public static StructureItem Serialize(object obj)
    {
        if (SerializationTable.TryGetValue(obj.GetType(), out ItemConstructor? constructor))
            return constructor.Invoke(obj);

        switch (obj)
        {
            case Dictionary<string, object> dictionary:
                return new CompoundItem(dictionary.Select(kv => KeyValuePair.Create(kv.Key, Serialize(kv.Value))).ToDictionary());
            case List<object> list:
                return new ListItem(list.Select(Serialize).ToList());
            case IStructureItemSerializable serializable:
                return serializable.Serialize();
        }

        if (obj.GetType().GetCustomAttribute<PdsAutoSerializableAttribute>() is null)
            return EofItem.Instance;
        
        CompoundItem compound = new();
        foreach (FieldInfo field in obj.GetType().GetFields())
            if (field.GetCustomAttribute<PdsNonSerializableAttribute>() is null)
                compound.Put(field.Name, field.GetValue(obj));
        return compound;
    }

    public static object? Deserialize(StructureItem structureItem)
    {
        if (DeserializationTable.TryGetValue(structureItem.GetType(), out ItemDeconstructor? deconstructor))
            return deconstructor.Invoke(structureItem);

        return structureItem switch
        {
            CompoundItem => structureItem.DictionaryValue
                .Select(kv => KeyValuePair.Create(kv.Key, Deserialize(kv.Value)))
                .ToDictionary(),
            ListItem => structureItem.ListValue.Select(Deserialize).ToList(),
            _ => null
        };
    }

    protected abstract void Read(BinaryReader reader);
    protected abstract void Write(BinaryWriter writer);

    public static StructureItem ReadItem(BinaryReader reader)
    {
        byte type = reader.ReadByte();

        if (type == 0 || type > RegisteredTypes.Length)
            return EofItem.Instance;

        if (RegisteredTypes[type - 1].GetConstructor([])?.Invoke(null) is not StructureItem item)
            return EofItem.Instance;

        item.Read(reader);
        return item;
    }

    public void WriteItem(BinaryWriter writer)
    {
        if (this is EofItem)
        {
            writer.Write((byte)255);
            return;
        }

        writer.Write(GetTypeId());
        Write(writer);
    }

    public static StructureItem ReadFromPath(string path,
        CompressionLevel compression = CompressionLevel.NoCompression)
    {
        using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
        Stream stream = fileStream;

        if (compression != CompressionLevel.NoCompression)
        {
            using GZipStream gZipStream = new(fileStream, compression);
            stream = gZipStream;
        }

        using BinaryReader binaryReader = new(stream);
        return ReadItem(binaryReader);
    }

    public void WriteToPath(string path,
        CompressionLevel compression = CompressionLevel.NoCompression)
    {
        DirectoryInfo? parentDirectory = Directory.GetParent(path);
        if (parentDirectory is null)
            throw new IOException($"Path \"{path}\" is invalid!");
        
        Directory.CreateDirectory(parentDirectory.FullName);
        
        using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
        Stream stream = fileStream;

        if (compression != CompressionLevel.NoCompression)
        {
            using GZipStream gZipStream = new(fileStream, compression);
            stream = gZipStream;
        }

        using BinaryWriter binaryWriter = new(stream);
        WriteItem(binaryWriter);
    }

    internal delegate StructureItem ItemConstructor(object value);

    internal delegate object? ItemDeconstructor(StructureItem value);

    static StructureItem()
    {
        RegisteredTypeIDs = RegisteredTypes
            .Select((type, i) => (type, (byte)(i + 1)))
            .ToDictionary();
    }
}

public class EofItem : StructureItem
{
    public static readonly EofItem Instance = new();

    private EofItem() { }

    protected override void Read(BinaryReader reader) { }

    protected override void Write(BinaryWriter writer) { }
}

public abstract class StructureItem<T>(T value) : StructureItem
{
    public T Value = value;

    public override string? ToString() => Value?.ToString();
}