using System.Net;
using VoxelThing.Game.Networking;

namespace VoxelThing.Server;

public class Client : IDisposable
{
    public readonly Connection Connection;
    public readonly ServerPacketHandler PacketHandler;

    public string? DisplayName;

    public IPEndPoint IpEndPoint => Connection.IpEndPoint;
    
    public Client(GameServer server, Connection connection)
    {
        Connection = connection;
        PacketHandler = new(server, this);
    }

    public void SendPacket(IPacket packet) => Connection.SendPacket(packet);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Connection.Dispose();
    }
}