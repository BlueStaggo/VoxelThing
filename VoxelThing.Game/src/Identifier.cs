using PDS;

namespace VoxelThing.Game;

public readonly record struct Identifier(string Namespace, string Name) : IStructureItemSerializable
{
    public Identifier(string name) : this("vt", name) { }

    public static Identifier FromFullName(string fullName)
    {
        string[] nameParts = fullName.Split(':', 2);
        return new Identifier(nameParts[0], nameParts[1]);
    }

    public string FullName { get; } = $"{Namespace}:{Name}";
    
    public StructureItem Serialize() => new StringItem(FullName);

    public static Identifier? Deserialize(StructureItem structureItem)
    {
        if (structureItem.StringValue is not { } fullName) return null;
        if (!fullName.Contains(':')) return null;
        return FromFullName(fullName);
    }
}