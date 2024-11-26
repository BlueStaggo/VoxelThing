using PDS;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds;

// [PdsAutoSerializable]
public class WorldInfo : IStructureItemSerializable
{
    public string Name = "World";
    public ulong Seed = new Random64().NextUInt64();

    public StructureItem Serialize() => new CompoundItem()
        .Put(nameof(Name), Name)
        .Put(nameof(Seed), Seed);
    
    public WorldInfo Deserialize(StructureItem? item)
    {
        if (item is not CompoundItem compound)
            return this;
        
        Name = compound[nameof(Name)]?.TryStringValue ?? Name;
        Seed = compound[nameof(Seed)]?.TryULongValue ?? Seed;
    
        return this;
    }
}