using VoxelThing.Game.Blocks;

namespace VoxelThing.Game.Worlds.Chunks;

public class EmptyChunk : Chunk
{
    public static readonly EmptyChunk Instance = new();

    private EmptyChunk() : base(null!, 0, 0, 0) // null!: We do a little trolling
    {
        MarkNoSave();
    }

    public override Block? GetBlock(int x, int y, int z) => null;

    public override void SetBlock(int x, int y, int z, Block? block) { }
}