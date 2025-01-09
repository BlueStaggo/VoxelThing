using MemoryPack;

namespace VoxelThing.Game.Networking;

[MemoryPackable]
public readonly partial record struct CSendMessagePacket(string Message) : IClientPacket<CSendMessagePacket>
{ public static ushort StaticId => 1; }

[MemoryPackable]
public readonly partial record struct CUpdateDisplayNamePacket(string DisplayName) : IClientPacket<CUpdateDisplayNamePacket>
{ public static ushort StaticId => 2; }