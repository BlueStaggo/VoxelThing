using VoxelThing.Game.Entities;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Server.Worlds;

public class ServerWorld(GameServer server, ISaveHandler saveHandler, WorldInfo? info = null) : World(saveHandler, info)
{
    public override void AddEntity(Entity entity)
    {
        base.AddEntity(entity);
        server.SendPacketToAllClients(new SAddEntity(entity));
    }

    protected override void TickEntity(Entity entity)
    {
        base.TickEntity(entity);
        if (entity.Position.HasChanged || entity.Yaw.HasChanged || entity.Pitch.HasChanged)
        {
            Console.WriteLine("MOVEMENT REQUESTED");
            server.SendPacketToAllClients(new SMoveEntity(entity));
        }
    }
}