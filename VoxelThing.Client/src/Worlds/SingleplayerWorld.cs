using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Worlds;

public class SingleplayerWorld(Game game, ISaveHandler saveHandler, WorldInfo? info = null)
    : World(saveHandler, info)
{
    public readonly Game Game = game;

    public override void OnBlockUpdate(int x, int y, int z)
    {
        base.OnBlockUpdate(x, y, z);
        Game.MainRenderer.WorldRenderer.MarkNeighborUpdateAt(x, y, z);
    }
    
    public override void OnChunkAdded(int x, int y, int z)
    {
        base.OnChunkAdded(x, y, z);
        Game.MainRenderer.WorldRenderer.MarkNeighborChunkUpdateAt(x, y, z);
    }
}