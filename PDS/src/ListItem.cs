
namespace PDS;

public class ListItem(List<StructureItem> value) : StructureItem<List<StructureItem>>(value)
{
    public override List<StructureItem> ListValue => Value;

    public ListItem() : this([]) { }

    protected override void Read(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        for (int i = 0; i < length; i++)
            Value.Add(ReadItem(reader));
    }

    protected override void Write(BinaryWriter writer)
    {
        writer.Write(Value.Count);
        foreach (StructureItem item in Value)
            item.WriteItem(writer);
    }
}
