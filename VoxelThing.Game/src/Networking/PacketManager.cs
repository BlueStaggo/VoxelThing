using System.Diagnostics.CodeAnalysis;

namespace VoxelThing.Game.Networking;

public class PacketManager
{
    public const int ProtocolVersion = 1;
    
    public static readonly PacketManager Client = new();
    public static readonly PacketManager Server = new();
    
    private readonly Dictionary<ushort, Type> packetIdMap = [];

    public static PacketManager ForSide(PacketSide side)
        => side switch
        {
            PacketSide.Client => Client,
            PacketSide.Server => Server,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    
    private PacketManager Register<TPacket>()
        where TPacket : IPacket
    {
        if (!Extensions.IsMemoryPackable(typeof(TPacket)))
            throw new ArgumentException($"Packet type \"{typeof(TPacket).Name}\" is not MessagePackable");
        
        ushort id = TPacket.StaticId;
        packetIdMap[id] = typeof(TPacket);
        return this;
    }

    public bool TryGetPacketTypeFromId(ushort id, [NotNullWhen(true)] out Type? type)
        => packetIdMap.TryGetValue(id, out type);
    
    public static void Init() { } // Dummy method for initializing global PacketManagers
    
    static PacketManager()
    {
        Client
            .Register<CHandshake>()
            .Register<CRequestConnection>()
            .Register<CUpdatePosition>()
            .Register<CSetBlock>()
            .Register<CThrowBouncy>()
        ;
        Server
            .Register<SHandshake>()
            .Register<SConnectionAccepted>()
            .Register<SDisconnect>()
            .Register<SSendMessage>()
            .Register<SLoadChunk>()
            .Register<SSetBlock>()
            .Register<SAddEntity>()
            .Register<SMoveEntity>()
        ;
    }
}
