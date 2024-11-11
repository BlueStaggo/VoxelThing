using PDS;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds.Chunks.Storage;

namespace VoxelThing.Game.Worlds.Chunks;

public class Chunk(World world, int x, int y, int z, BlockArray blockArray) : IBlockAccess, IStructureItemSerializable
{
    public const int SizePow2 = 5;
    public const int Length = 1 << SizePow2;
    public const int LengthMask = (1 << SizePow2) - 1;
    public const int Area = 1 << SizePow2 * 2;
    public const int Volume = 1 << SizePow2 * 3;

    public readonly World World = world;
    public readonly int X = x, Y = y, Z = z;

    public int BlockX => X * Length;
    public int BlockY => Y * Length;
    public int BlockZ => Z * Length;
    
    private BlockArray blockArray = blockArray;
    private int blockCount;
    private bool hasChanged;

    public bool Empty => blockCount <= 0;

    public Chunk(World world, int x, int y, int z)
        : this(world, x, y, z, EmptyBlockArray.Instance) { }

    public int ToGlobalX(int x) => x + (X << SizePow2);
    public int ToGlobalY(int y) => y + (Y << SizePow2);
    public int ToGlobalZ(int z) => z + (Z << SizePow2);

    public int ToLocalX(int x) => x - (X << SizePow2);
    public int ToLocalY(int y) => y - (Y << SizePow2);
    public int ToLocalZ(int z) => z - (Z << SizePow2);

    public virtual Block? GetBlock(int x, int y, int z) => blockArray.GetBlock(x, y, z);

    public virtual void SetBlock(int x, int y, int z, Block? block)
    {
        while (blockArray.RequiresExpansion(block))
            blockArray = blockArray.Expand();
        
        if (block is null && blockArray.GetBlockId(x, y, z) != 0)
            blockCount--;
        else if (block is not null && blockArray.GetBlockId(x, y, z) == 0)
            blockCount++;

        blockArray.SetBlock(x, y, z, block);
        hasChanged = true;
    }

    public void MarkNoSave() => hasChanged = false;
    public void OnUnload() => Save();

    public void Save()
    {
        if (hasChanged && Serialize() is CompoundItem compoundItem)
            World.SaveHandler.SaveChunkData(X, Y, Z, compoundItem);
    }

    public StructureItem Serialize()
    => new CompoundItem()
        .Put("Blocks", blockArray);

    public static Chunk Deserialize(World world, int x, int y, int z, CompoundItem compound)
    {
        BlockArray blockArray = BlockArray.Deserialize(compound["Blocks"]);
        return new Chunk(world, x, y, z, blockArray);
    }
}