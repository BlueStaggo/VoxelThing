using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Worlds;

public class RemoteWorld : SingleplayerWorld
{
    public RemoteWorld(Game game, ISaveHandler saveHandler, WorldInfo? info = null) : base(game, saveHandler, info)
    {
        Remote = true;
        ChunkStorage = new RemoteChunkStorage(this);
    }
}