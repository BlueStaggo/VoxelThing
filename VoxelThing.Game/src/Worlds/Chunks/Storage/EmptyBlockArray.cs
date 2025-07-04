using MemoryPack;
using PDS;
using VoxelThing.Game.Blocks;

namespace VoxelThing.Game.Worlds.Chunks.Storage;

[MemoryPackable]
public partial class EmptyBlockArray : BlockArray
{
    public static readonly EmptyBlockArray Instance = new();

    protected override int MaxPaletteSize => 0;

    protected internal override int GetBlockId(int x, int y, int z) => 0;
    protected internal override void SetBlockId(int x, int y, int z, int id) { }
    public override void Deserialize(CompoundItem compoundItem) { }

    public override bool RequiresExpansion(Block? block) => block is not null;

    protected override BlockArray GetExpandedBlockArray() => new NibbleBlockArray()
    {
        Palette = Palette
    };
}