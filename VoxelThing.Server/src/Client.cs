using System.Net;
using VoxelThing.Game.Entities;
using VoxelThing.Game.Networking;
using VoxelThing.Server.Worlds;

namespace VoxelThing.Server;

public class Client : IDisposable
{
    public readonly Connection Connection;
    public readonly GameServer Server;
    public readonly ServerPacketHandler PacketHandler;

    public string? Username;
    public Player? Character;

    public IPEndPoint IpEndPoint => Connection.IpEndPoint;

    private readonly PlayerIsland Island;
    
    public Client(GameServer server, Connection connection)
    {
        Connection = connection;
        Server = server;
        PacketHandler = new(server, this);
        Island = new(this);
    }

    public void Update()
    {
        Island.Update();
    }

    public void SendPacket(IPacket packet) => Connection.SendPacket(packet);

    public void Disconnect(string reason)
    {
        Connection.SendPacket(new SDisconnect(reason));
        Server.DisconnectClient(Connection);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Connection.Dispose();
    }
}