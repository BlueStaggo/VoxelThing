using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client.Worlds;

public class SingleplayerWorld : World
{
    public readonly Game Game;

    public SingleplayerWorld(Game game, ISaveHandler saveHandler, WorldInfo? info = null) : base(saveHandler, info)
    {
        Game = game;
        Profiler = Game.Profiler;
    }

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