// ReSharper disable ArrangeTypeMemberModifiers

using VoxelThing.Game.Blocks;
using VoxelThing.Game.Entities;
using VoxelThing.Game.Networking;

namespace VoxelThing.Server;

public sealed class ServerPacketHandler : PacketHandler
{
    public override PacketSide Side => PacketSide.Server;

    public readonly GameServer Server;
    public readonly Client Client;
    
    public ServerPacketHandler(GameServer server, Client client)
    {
        Server = server;
        Client = client;
        
        Register<CHandshake>(CHandshake);
        Register<CRequestConnection>(CRequestConnection);
        Register<CUpdatePosition>(CUpdatePosition);
        Register<CSetBlock>(CSetBlock);
        Register<CThrowBouncy>(CThrowBouncy);
    }

    void CHandshake(CHandshake packet)
    {
        Client.SendPacket(new SHandshake());
    }

    void CRequestConnection(CRequestConnection packet)
    {
        if (packet.ProtocolVersion != PacketManager.ProtocolVersion)
        {
            Client.Disconnect($"Invalid protocol version. Attempted to join with version {packet.ProtocolVersion}" +
                              $"on a server with version {PacketManager.ProtocolVersion}.");
            return;
        }

        Client.Username = packet.Username;
        Client.Character = new Player(Server.World);
        Client.SendPacket(new SConnectionAccepted());
        
        foreach (Client otherClient in Server.Clients.Values)
            if (otherClient != Client && otherClient.Character is not null)
                Client.SendPacket(new SAddEntity(otherClient.Character));
        
        Server.SendPacketToOtherClients(new SAddEntity(Client.Character), Client);
        Server.SendPacketToAllClients(new SSendMessage($"§cffff55{packet.Username} has joined."));
    }

    void CUpdatePosition(CUpdatePosition packet)
    {
        if (Client.Character is null)
            return;

        Client.Character.Position.Value = packet.Position;
        Client.Character.Yaw.Value = packet.Yaw;
        Client.Character.Pitch.Value = packet.Pitch;
        Server.SendPacketToOtherClients(new SMoveEntity(Client.Character), Client);
    }

    void CSetBlock(CSetBlock packet)
    {
        if (Client.Character is null)
            return;

        Block? block = Block.FromId(packet.Block);
        Server.World.SetBlock(packet.X, packet.Y, packet.Z, block);
        Server.SendPacketToOtherClients(new SSetBlock(packet), Client);
    }

    void CThrowBouncy(CThrowBouncy packet)
    {
        if (Client.Character is null)
            return;

        BouncyEntity bouncy = new(Server.World);
        bouncy.Position.JumpTo(Client.Character.Position);
        bouncy.Velocity.JumpTo(Client.Character.GetLookVector());
        Server.World.AddEntity(bouncy);
    }
}