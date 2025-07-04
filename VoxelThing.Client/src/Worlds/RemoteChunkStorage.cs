using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Worlds;

public class RemoteChunkStorage(World world) : ChunkStorage(world)
{
    protected override Chunk? LoadChunk(int x, int y, int z) => null;
}