using PDS;
using VoxelThing.Game.Blocks;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

public abstract class BlockArray : IStructureItemSerializable
{
    private static readonly Func<BlockArray>[] RegisteredSuppliers =
    [
        () => new NibbleBlockArray(),
        () => new ByteBlockArray(),
        () => new TriNibbleBlockArray(),
        () => new ShortBlockArray(),
    ];

    private static readonly Dictionary<Type, int> RegisteredTypeIds = [];

    protected internal List<Block?> Palette = [];
    
    protected abstract int MaxPaletteSize { get; }
    protected virtual CompoundItem SerializedData => new();

    protected internal abstract int GetBlockId(int x, int y, int z);
    protected internal abstract void SetBlockId(int x, int y, int z, int id);

    static BlockArray()
    {
        int id = 0;
        RegisteredTypeIds[typeof(EmptyBlockArray)] = id++;
        RegisteredTypeIds[typeof(NibbleBlockArray)] = id++;
        RegisteredTypeIds[typeof(ByteBlockArray)] = id++;
        RegisteredTypeIds[typeof(TriNibbleBlockArray)] = id++;
        RegisteredTypeIds[typeof(ShortBlockArray)] = id++;
    }

    public virtual BlockArray Expand()
    {
        BlockArray nextBlockArray = GetExpandedBlockArray();

        for (int x = 0; x < Chunk.Length; x++)
        for (int y = 0; y < Chunk.Length; y++)
        for (int z = 0; z < Chunk.Length; z++)
            nextBlockArray.SetBlockId(x, y, z, GetBlockId(x, y, z));
        
        return nextBlockArray;
    }

    protected virtual BlockArray GetExpandedBlockArray()
        => throw new InvalidOperationException("Cannot expand to larger block storage!");

    public virtual bool RequiresExpansion(Block? block)
        => Palette.Count >= MaxPaletteSize && !Palette.Contains(block);

    public Block? GetBlock(int x, int y, int z)
    {
        int id = GetBlockId(x, y, z);
        if (id == 0 || id > Palette.Count) return null;
        return Palette[id - 1];
    }

    public void SetBlock(int x, int y, int z, Block? block)
    {
        if (block is null)
        {
            SetBlockId(x, y, z, 0);
            return;
        }

        int id = Palette.IndexOf(block) + 1;
        if (id == 0)
        {
            if (Palette.Count >= MaxPaletteSize)
                throw new InvalidOperationException("Cannot add \"" + block + "\" to palette: ran out of " + MaxPaletteSize + "spaces!");
            
            id = Palette.LastIndexOf(null) + 1;
            if (id <= 0)
            {
                Palette.Add(block);
                id = Palette.Count;
            }
            else
            {
                Palette[id - 1] = block;
            }
        }

        SetBlockId(x, y, z, id);
    }

    public abstract void Deserialize(CompoundItem compoundItem);

    public StructureItem Serialize()
        => SerializedData
            .Put("Type", (byte) RegisteredTypeIds[GetType()])
            .Put("Palette", Palette.Select(b => b?.Id.FullName ?? Block.AirId.FullName).ToList());

    public static BlockArray Deserialize(StructureItem? structureItem)
    {
        if (structureItem is not CompoundItem compoundItem)
            return EmptyBlockArray.Instance;
        
        byte type = compoundItem["Type"]?.TryByteValue ?? 0;
        if (type == 0 || type > RegisteredTypeIds.Count)
            return EmptyBlockArray.Instance;
        
        BlockArray blockArray = RegisteredSuppliers[type - 1].Invoke();
        blockArray.Palette = compoundItem["Palette"]?.TryListValue?
            .Select(s => Block.FromId(Identifier.FromFullName(s.StringValue)))
            .ToList() ?? [];
        blockArray.Deserialize(compoundItem);

        return blockArray;
    }
}
