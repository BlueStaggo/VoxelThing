using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Worlds;

public readonly struct WorldCache : IBlockAccess
{
    private const int Margin = 1;
    private const int Margin2 = Margin * 2;
    
    private readonly World world;
    private readonly Chunk?[,,] chunks = new Chunk?[3, 3, 3];
    private readonly Block?[,,] blocks = new Block?[Chunk.Length + Margin2, Chunk.Length + Margin2, Chunk.Length + Margin2];
    private readonly int centerX, centerY, centerZ;

    public WorldCache(World world, int centerX, int centerY, int centerZ)
    {
        this.world = world;
        this.centerX = centerX;
        this.centerY = centerY;
        this.centerZ = centerZ;

        for (int x = 0; x < 3; x++)
        for (int y = 0; y < 3; y++)
        for (int z = 0; z < 3; z++)
        {
            Chunk? chunk = world.GetChunkAt(centerX + x - 1, centerY + y - 1, centerZ + z - 1);
            chunks[x, y, z] = chunk;
        }

        for (int x = 0; x < Chunk.Length + Margin2; x++)
        {
            int gx = x + (centerX << Chunk.LengthPow2) - Margin;
            for (int y = 0; y < Chunk.Length + Margin2; y++)
            {
                int gy = y + (centerY << Chunk.LengthPow2) - Margin;
                for (int z = 0; z < Chunk.Length + Margin2; z++)
                {
                    int gz = z + (centerZ << Chunk.LengthPow2) - Margin;
                    blocks[x, y, z] = GetChunkAtBlock(gx, gy, gz)
                        .GetBlock(gx & Chunk.LengthMask, gy & Chunk.LengthMask, gz & Chunk.LengthMask);
                }
            }
        }
    }

    private Chunk GetChunkAtBlock(int x, int y, int z)
    {
        int ix = (x >> Chunk.LengthPow2) - (centerX - 1);
        int iy = (y >> Chunk.LengthPow2) - (centerY - 1);
        int iz = (z >> Chunk.LengthPow2) - (centerZ - 1);
        Chunk? chunk;

        if (ix < 0 || ix > 2 || iy < 0 || iy > 2 || iz < 0 || iz > 2)
            chunk = world.GetChunkAtBlock(x, y, z);
        else
            chunk = chunks[ix, iy, iz];
        
        return chunk ?? EmptyChunk.Instance;
    }

    public Block? GetBlock(int x, int y, int z)
    {
        int lx = x - centerX * Chunk.Length + Margin;
        int ly = y - centerY * Chunk.Length + Margin;
        int lz = z - centerZ * Chunk.Length + Margin;
        if (lx < 0 || ly < 0 || lz < 0
            || lx > Chunk.Length + Margin2 || ly > Chunk.Length + Margin2 || lz > Chunk.Length + Margin2)
            return GetChunkAtBlock(x, y, z).GetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask);

        return blocks[lx, ly, lz];
    }
}