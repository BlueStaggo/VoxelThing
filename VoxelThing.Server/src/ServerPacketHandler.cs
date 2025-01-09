// ReSharper disable ArrangeTypeMemberModifiers

using VoxelThing.Game.Networking;

namespace VoxelThing.Server;

public sealed class ServerPacketHandler : PacketHandler
{
    private readonly GameServer server;
    private readonly Client client;
    
    public ServerPacketHandler(GameServer server, Client client)
    {
        this.server = server;
        this.client = client;
        
        Register<CSendMessagePacket>(SendMessage);
        Register<CUpdateDisplayNamePacket>(UpdateDisplayName);
    }

    void SendMessage(CSendMessagePacket packet)
    {
        string author = client.DisplayName ?? client.IpEndPoint.ToString();
        Console.WriteLine($"{author}: {packet.Message}");
        
        foreach (Client peer in server.Clients.Values)
            peer.SendPacket(new SSendMessagePacket(author, packet.Message));
    }

    void UpdateDisplayName(CUpdateDisplayNamePacket packet)
    {
        client.DisplayName = packet.DisplayName;
        Console.WriteLine($"{client.IpEndPoint} set their display name to {packet.DisplayName}");
    }
}