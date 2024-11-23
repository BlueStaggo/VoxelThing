using PDS;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds.Chunks.Storage;

namespace VoxelThing.Game.Worlds.Chunks;

public class Chunk : IBlockAccess, IStructureItemSerializable
{
    public const int LengthPow2 = 5;
    public const int Length = 1 << LengthPow2;
    public const int LengthMask = (1 << LengthPow2) - 1;
    public const int Area = 1 << LengthPow2 * 2;
    public const int Volume = 1 << LengthPow2 * 3;

    public readonly World World;
    public readonly int X, Y, Z;

    public int BlockX => X * Length;
    public int BlockY => Y * Length;
    public int BlockZ => Z * Length;
    
    private BlockArray blockArray;
    private int blockCount;
    private bool hasChanged;

    public bool Empty => blockCount <= 0;

    public Chunk(World world, int x, int y, int z)
        : this(world, x, y, z, EmptyBlockArray.Instance) { }

    public Chunk(World world, int x, int y, int z, BlockArray blockArray)
    {
        World = world;
        X = x;
        Y = y;
        Z = z;
        this.blockArray = blockArray;
        
        for (int xx = 0; xx < Length; xx++)
        for (int yy = 0; yy < Length; yy++)
        for (int zz = 0; zz < Length; zz++)
            if (blockArray.GetBlockId(x, y, z) != 0)
                blockCount++;
    }

    public int ToGlobalX(int x) => x + (X << LengthPow2);
    public int ToGlobalY(int y) => y + (Y << LengthPow2);
    public int ToGlobalZ(int z) => z + (Z << LengthPow2);

    public int ToLocalX(int x) => x - (X << LengthPow2);
    public int ToLocalY(int y) => y - (Y << LengthPow2);
    public int ToLocalZ(int z) => z - (Z << LengthPow2);

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