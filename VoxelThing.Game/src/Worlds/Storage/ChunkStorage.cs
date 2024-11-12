using OpenTK.Mathematics;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Utils;

namespace VoxelThing.Game.Worlds.Storage;

public class ChunkStorage(World world)
{
    public const int DiameterPow2 = 5;
    public const int Diameter = 1 << DiameterPow2;
    public const int DiameterMask = Diameter - 1;

    protected readonly World World = world;
    protected readonly Dictionary<Vector3i, Chunk> Chunks = [];

    public Profiler? Profiler => World.Profiler; 

    public Chunk? GetChunkAt(int x, int y, int z)
    {
        Vector3i key = new(x, y, z);
        if (Chunks.TryGetValue(key, out Chunk? chunk) && chunk.X == x && chunk.Y == y && chunk.Z == z)
            return chunk;
        
        chunk?.OnUnload();
        chunk = Chunks[key] = LoadChunk(x, y, z);
        return chunk;
    }

    protected Chunk LoadChunk(int x, int y, int z)
    {
        Profiler?.Push("load-chunk");
        if (World.SaveHandler.LoadChunkData(x, y, z) is { } chunkData)
            return Chunk.Deserialize(World, x, y, z, chunkData);
            
        Chunk chunk = new(World, x, y, z);
        World.GenerateChunk(chunk);
        chunk.MarkNoSave();
        World.OnChunkAdded(x, y, z);
        
        Profiler?.Pop();
        return chunk;
    }

    public void UnloadAllChunks()
    {
        foreach (Chunk? chunk in Chunks.Values)
            chunk?.OnUnload();
        Chunks.Clear();
    }

    public bool ChunkExists(int x, int y, int z)
        => Chunks.TryGetValue(new(x, y, z), out Chunk? chunk) && chunk.X == x && chunk.Y == y && chunk.Z == z;
}
