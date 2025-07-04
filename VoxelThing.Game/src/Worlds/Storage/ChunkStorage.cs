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
        if (Chunks.TryGetValue(key, out Chunk? chunk))
            return chunk;
        
        chunk?.OnUnload();
        chunk = LoadChunk(x, y, z);
        if (chunk is not null)
            Chunks[key] = chunk;
        return chunk;
    }

    public void AddChunk(Chunk chunk)
    {
        Vector3i key = new(chunk.X, chunk.Y, chunk.Z);
        if (Chunks.TryGetValue(key, out Chunk? prevChunk))
            prevChunk.OnUnload();
        Chunks[key] = chunk;
    }

    protected virtual Chunk? LoadChunk(int x, int y, int z)
    {
        Chunk chunk;
        
        Profiler?.Push("load-chunk");
        if (World.SaveHandler.LoadChunkData(x, y, z) is { } chunkData)
        {
            chunk = Chunk.Deserialize(World, x, y, z, chunkData);
            Profiler?.Pop();
            return chunk;
        }
            
        chunk = new(World, x, y, z);
        Profiler?.PopPush("generate-chunk");
        World.GenerateChunk(chunk, Profiler);
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

    public void UnloadSurroundingChunks(int cx, int cy, int cz, int distanceH, int distanceV)
    {
        Vector3i centerLocation = new(cx, cy, cz);
        
        List<Vector3i> chunkLocations = Chunks.Keys.ToList();
        foreach (Vector3i chunkLocation in chunkLocations)
        {
            Vector3i chunkDistance = chunkLocation - centerLocation;
            if (Math.Abs(chunkDistance.X) > distanceH
                || Math.Abs(chunkDistance.Y) > distanceV
                || Math.Abs(chunkDistance.Z) > distanceH)
            {
                Chunk chunk = Chunks[chunkLocation];
                chunk.OnUnload();
                Chunks.Remove(chunkLocation);
            }
        }
    }

    public bool ChunkExists(int x, int y, int z)
        => Chunks.TryGetValue(new(x, y, z), out Chunk? chunk) && chunk.X == x && chunk.Y == y && chunk.Z == z;
}
