using MemoryPack;
using OpenTK.Mathematics;
using VoxelThing.Game.Entities;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Worlds.Chunks.Storage;

namespace VoxelThing.Game.Networking;

[MemoryPackable]
public readonly partial record struct SHandshake() : IServerPacket<SHandshake>
{ public static ushort StaticId => 1; }

[MemoryPackable]
public readonly partial record struct SConnectionAccepted() : IServerPacket<SConnectionAccepted>
{ public static ushort StaticId => 2; }

[MemoryPackable]
public readonly partial record struct SDisconnect(string Reason) : IServerPacket<SDisconnect>
{ public static ushort StaticId => 3; }

[MemoryPackable]
public readonly partial record struct SSendMessage(string Message) : IServerPacket<SSendMessage>
{ public static ushort StaticId => 4; }

[MemoryPackable]
[method: MemoryPackConstructor]
public readonly partial record struct SLoadChunk(int X, int Y, int Z, BlockArray BlockArray) : IServerPacket<SLoadChunk>
{
    public static ushort StaticId => 5;

    public SLoadChunk(Chunk chunk) : this(chunk.X, chunk.Y, chunk.Z, chunk.BlockArray) { }
}

[MemoryPackable]
[method: MemoryPackConstructor]
public readonly partial record struct SSetBlock(int X, int Y, int Z, Identifier Block) : IServerPacket<SSetBlock>
{
    public static ushort StaticId => 6;
    
    public SSetBlock(CSetBlock packet) : this(packet.X, packet.Y, packet.Z, packet.Block) { }
}

[MemoryPackable]
[method: MemoryPackConstructor]
public readonly partial record struct SAddEntity(Guid Guid, string? Type, Vector3d Position, double Yaw, double Pitch)
    : IServerPacket<SAddEntity>
{
    public static ushort StaticId => 7;
    
    public SAddEntity(Entity entity) : this(entity.Guid, entity.Type, entity.Position, entity.Yaw, entity.Pitch) { }
}

[MemoryPackable]
public readonly partial record struct SMoveEntity(Guid Guid, Vector3d Position, double Yaw, double Pitch)
    : IServerPacket<SMoveEntity>
{
    public static ushort StaticId => 8;
    
    public SMoveEntity(Entity entity) : this(entity.Guid, entity.Position, entity.Yaw, entity.Pitch) { }
}