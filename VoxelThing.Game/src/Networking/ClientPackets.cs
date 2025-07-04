using MemoryPack;
using OpenTK.Mathematics;
using VoxelThing.Game.Entities;

namespace VoxelThing.Game.Networking;

[MemoryPackable]
public readonly partial record struct CHandshake() : IClientPacket<CHandshake>
{
    public static ushort StaticId => 1;
    public static readonly CHandshake Instance = new();
}

[MemoryPackable]
public readonly partial record struct CRequestConnection(int ProtocolVersion, string Username) : IClientPacket<CRequestConnection>
{ public static ushort StaticId => 2; }

[MemoryPackable]
public readonly partial record struct CUpdatePosition(Vector3d Position, double Yaw, double Pitch)
    : IClientPacket<CUpdatePosition>
{
    public static ushort StaticId => 3;
    
    public CUpdatePosition(Entity entity) : this(entity.Position, entity.Yaw, entity.Pitch) { }
}

[MemoryPackable]
public readonly partial record struct CSetBlock(int X, int Y, int Z, Identifier Block) : IClientPacket<CSetBlock>
{ public static ushort StaticId => 4; }

[MemoryPackable]
public readonly partial record struct CThrowBouncy() : IClientPacket<CThrowBouncy>
{ public static ushort StaticId => 5; }