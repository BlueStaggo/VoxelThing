using MemoryPack;

namespace VoxelThing.Game.Networking;

[MemoryPackable]
public readonly partial record struct CSendMessagePacket(string Message) : IClientPacket<CSendMessagePacket>
{ public static ushort Id => 1; }

[MemoryPackable]
public readonly partial record struct CUpdateDisplayName(string DisplayName) : IClientPacket<CUpdateDisplayName>
{ public static ushort Id => 2; }