using PDS;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Storage;

public class LegacyChunkStorage(World world)
{
    public const int DiameterPow2 = 5;
    public const int Diameter = 1 << DiameterPow2;
    public const int DiameterMask = Diameter - 1;

    protected readonly World World = world;
    protected readonly Chunk?[,,] Chunks = new Chunk?[Diameter, Diameter, Diameter];

    public Chunk? GetChunkAt(int x, int y, int z)
    {
        int mx = x & DiameterMask;
        int my = y & DiameterMask;
        int mz = z & DiameterMask;
        Chunk? chunk = Chunks[mx, my, mz];
        
        if (chunk is not null && chunk.X == x && chunk.Y == y && chunk.Z == z)
            return chunk;
        
        chunk?.OnUnload();
        chunk = Chunks[mx, my, mz] = LoadChunk(x, y, z);
        return chunk;
    }

    protected Chunk LoadChunk(int x, int y, int z)
    {
        if (World.SaveHandler.LoadChunkData(x, y, z) is { } chunkData)
            return Chunk.Deserialize(World, x, y, z, chunkData);
            
        Chunk chunk = new(World, x, y, z);
        World.GenerateChunk(chunk);
        chunk.MarkNoSave();
        World.OnChunkAdded(x, y, z);
        Console.WriteLine($"Update {x}, {y}, {z}");
        return chunk;
    }

    public void UnloadAllChunks()
    {
        foreach (Chunk? chunk in Chunks)
            chunk?.OnUnload();
        Array.Clear(Chunks);
    }

    public bool ChunkExists(int x, int y, int z)
    {
        int mx = x & DiameterMask;
        int my = y & DiameterMask;
        int mz = z & DiameterMask;
        Chunk? chunk = Chunks[mx, my, mz];
        return chunk is not null && chunk.X == x && chunk.Y == y && chunk.Z == z;
    }
}
