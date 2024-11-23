using PDS;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds;

[PdsAutoSerializable]
public class WorldInfo
{
    public string Name = "World";
    public ulong Seed = new Random64().NextUInt64();

    // public StructureItem Serialize() => new CompoundItem()
    //     .Put(nameof(Name), Name)
    //     .Put(nameof(Seed), Seed);
    //
    // public static WorldInfo Deserialize(StructureItem? item)
    // {
    //     WorldInfo info = new();
    //     if (item is not CompoundItem compound)
    //         return info;
    //     
    //     info.Name = compound[nameof(Name)]?.TryStringValue ?? info.Name;
    //     info.Seed = compound[nameof(Seed)]?.TryULongValue ?? info.Seed;
    //
    //     return info;
    // }
}