using MemoryPack;

namespace VoxelThing.Game.Networking;

public interface IPacket
{
    public static virtual ushort StaticId => throw new NotImplementedException("Packet must have an id!");
    public PacketSide Side { get; }
    public ushort Id { get; }

    public static IPacket? Read(BinaryReader reader, PacketSide senderSide)
    {
        ushort id = reader.ReadUInt16();
        if (!PacketManager.ForSide(senderSide).TryGetPacketTypeFromId(id, out Type? type))
            return null;
        
        ushort length = reader.ReadUInt16();
        if (length == 0)
            return null;
        
        object? packet = MemoryPackSerializer.Deserialize(type, reader.ReadBytes(length));
        return packet as IPacket;
    }

    public void Write(BinaryWriter writer);
}

public interface IPacket<T> : IPacket
    where T : IPacket<T>
{
    ushort IPacket.Id => T.StaticId;
    
    void IPacket.Write(BinaryWriter writer)
    {
        ushort id = T.StaticId;
        if (id == 0)
            return;
        byte[] serialized = MemoryPackSerializer.Serialize((T)this);
        if (serialized.Length > ushort.MaxValue)
            return;
        
        writer.Write(id);
        writer.Write((ushort)serialized.Length);
        writer.Write(MemoryPackSerializer.Serialize((T)this));
    }
}

public interface IClientPacket<T> : IPacket<T>
    where T : IPacket<T>
{
    PacketSide IPacket.Side => PacketSide.Client;
}
    
public interface IServerPacket<T> : IPacket<T>
    where T : IPacket<T>
{
    PacketSide IPacket.Side => PacketSide.Server;
}
