namespace VoxelThing.Game.Networking;

public abstract class PacketHandler
{
    private delegate void Callback(IPacket packet);
    
    public abstract PacketSide Side { get; }
    private readonly Dictionary<ushort, Callback> callbacks = [];

    protected void Register<TPacket>(Action<TPacket> callback) where TPacket : IPacket<TPacket>
        => callbacks[TPacket.StaticId] = packet =>
        {
            if (packet is not TPacket tpacket || packet.Side == Side)
                throw new ArgumentException("Attempted to register callback for invalid packet type", nameof(packet));
            callback(tpacket);
        };

    public void HandlePacket(IPacket packet)
    {
        if (SharedConstants.PrintPackets)
            Console.WriteLine("< " + packet);
        
        ushort id = packet.Id;
        if (callbacks.TryGetValue(id, out Callback? callback))
            callback(packet);
    }
    
    public void HandlePacket<TPacket>(TPacket packet)
        where TPacket : IPacket<TPacket>
    {
        ushort id = TPacket.StaticId;
        if (callbacks.TryGetValue(id, out Callback? callback))
            callback(packet);
    }
}