
using System.Reflection;

namespace PDS;

public class CompoundItem(Dictionary<string, StructureItem> value) : StructureItem<Dictionary<string, StructureItem>>(value)
{
    public override Dictionary<string, StructureItem> DictionaryValue => Value;

    public CompoundItem() : this([]) { }

    protected override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        for (int i = 0; i < length; i++)
            Value[reader.ReadString()] = ReadItem(reader);
    }

    protected override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Count);
        foreach (string key in Value.Keys)
        {
            writer.Write(key);
            Value[key].WriteItem(writer);
        }
    }

    public StructureItem? this[string index]
    {
        get => Value[index];
        set
        {
            if (value is null) Value.Remove(index);
            else Value[index] = value;
        }
    }

    public CompoundItem Put(string key, object? value)
    {
        if (value is null)
        {
            Value.Remove(key);
            return this;
        }

        this[key] = value as StructureItem ?? Serialize(value);
        return this;
    }

    public T? Deserialize<T>() => (T?)Deserialize(typeof(T));

    public object? Deserialize(Type type)
    {
        if (type.GetCustomAttribute<PdsAutoSerializableAttribute>() is null) return default;

        ConstructorInfo? constructor = type.GetConstructor([]);
        if (constructor is null) return default;

        object obj = constructor.Invoke(null);
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.GetCustomAttribute<PdsNonSerializableAttribute>() is not null) continue;

            StructureItem? structureItem = this[field.Name];
            if (structureItem is null) continue;

            object? value =
                structureItem is CompoundItem compound
                    ? compound.Deserialize(field.GetType())
                    : Deserialize(structureItem);
            if (value is null || value?.GetType() != field.FieldType) continue;

            field.SetValue(obj, value);
        }

        return obj;
    }
}
