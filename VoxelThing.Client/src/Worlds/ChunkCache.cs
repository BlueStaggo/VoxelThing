using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Worlds;

public readonly struct ChunkCache : IBlockAccess
{
    private readonly World world;
    private readonly Chunk?[,,] chunks = new Chunk?[3, 3, 3];
    private readonly int x, y, z;

    public ChunkCache(World world, int x, int y, int z)
    {
        this.world = world;
        this.x = x;
        this.y = y;
        this.z = z;

        for (int xx = 0; xx < 3; xx++)
        for (int yy = 0; yy < 3; yy++)
        for (int zz = 0; zz < 3; zz++)
            chunks[xx, yy, zz] = world.GetChunkAt(x + xx - 1, y + yy - 1, z + zz - 1);
    }

    private Chunk GetChunkAtBlock(int x, int y, int z)
    {
        int ix = (x >> Chunk.SizePow2) - (this.x - 1);
        int iy = (y >> Chunk.SizePow2) - (this.y - 1);
        int iz = (z >> Chunk.SizePow2) - (this.z - 1);
        Chunk? chunk;

        if (ix < 0 || ix > 2 || iy < 0 || iy > 2 || iz < 0 || iz > 2)
            chunk = world.GetChunkAtBlock(x, y, z);
        else
            chunk = chunks[ix, iy, iz];
        
        return chunk ?? EmptyChunk.Instance;
    }

    public Block? GetBlock(int x, int y, int z)
        => GetChunkAtBlock(x, y, z).GetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask);
}