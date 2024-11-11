
namespace PDS;

public class BoolItem(bool value) : StructureItem<bool>(value)
{
    public BoolItem() : this(false) { }
    
    public override bool BoolValue => Value;

    protected override void Read(BinaryReader reader)
        => Value = reader.ReadBoolean();

    protected override void Write(BinaryWriter writer)
        => writer.Write(Value);
}
