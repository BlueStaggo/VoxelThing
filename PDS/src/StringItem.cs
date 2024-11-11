
namespace PDS;

public class StringItem(string value) : StructureItem<string>(value)
{
    public override string StringValue => Value;

    public StringItem() : this(string.Empty) { }

    protected override void Read(BinaryReader reader)
        => Value = reader.ReadString();

    protected override void Write(BinaryWriter writer)
        => writer.Write(Value);
}
