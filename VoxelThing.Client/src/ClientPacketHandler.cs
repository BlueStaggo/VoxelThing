// ReSharper disable ArrangeTypeMemberModifiers

using VoxelThing.Client.Gui.Screens;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Entities;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Client;

public sealed class ClientPacketHandler : PacketHandler
{
    public override PacketSide Side => PacketSide.Client;

    public readonly Game Client;
    public readonly Connection Server;
    public int TimeSinceLastHandshake { get; private set; }
    
    public ClientPacketHandler(Game client, Connection server)
    {
        Client = client;
        Server = server;
        TimeSinceLastHandshake = Client.Ticks;
        
        Register<SHandshake>(SHandshake);
        Register<SConnectionAccepted>(SConnectionAccepted);
        Register<SDisconnect>(SDisconnect);
        Register<SLoadChunk>(SLoadChunk);
        Register<SSetBlock>(SSetBlock);
        Register<SAddEntity>(SAddEntity);
        Register<SMoveEntity>(SMoveEntity);
    }

    void SHandshake(SHandshake packet)
    {
        TimeSinceLastHandshake = Client.Ticks;
    }
    
    void SConnectionAccepted(SConnectionAccepted packet)
    {
        Client.StartWorld(EmptySaveHandler.Instance, true);
    }

    void SDisconnect(SDisconnect packet)
    {
        Client.ExitWorld();
        Client.CurrentScreen = new MultiplayerDisconnectionScreen(Client, packet.Reason);
    }

    void SLoadChunk(SLoadChunk packet)
    {
        if (Client.World is null)
            return;

        Chunk chunk = new Chunk(Client.World, packet.X, packet.Y, packet.Z, packet.BlockArray);
        Client.World.AddChunk(chunk);
    }

    void SSetBlock(SSetBlock packet)
    {
        Client.World?.SetBlock(packet.X, packet.Y, packet.Z, Block.FromId(packet.Block));
    }

    void SAddEntity(SAddEntity packet)
    {
        if (Client.World is null)
            return;

        Entity? entity = packet.Type switch
        {
            "entity" => new Entity(Client.World) { Guid = packet.Guid },
            "bouncy" => new BouncyEntity(Client.World) { Guid = packet.Guid },
            "player" => new Player(Client.World) { Guid = packet.Guid },
            _ => null
        };

        if (entity is null)
            return;
        Client.World.AddEntity(entity);
    }

    void SMoveEntity(SMoveEntity packet)
    {
        if (!(Client.World?.Entities.TryGetValue(packet.Guid, out Entity? entity) ?? false))
            return;

        entity.Position.Value = packet.Position;
        entity.Yaw.Value = packet.Yaw;
        entity.Pitch.Value = packet.Pitch;
    }
}