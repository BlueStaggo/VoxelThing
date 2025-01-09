using MemoryPack;

namespace VoxelThing.Game.Networking;

[MemoryPackable]
public readonly partial record struct SSendMessagePacket(string Author, string Message) : IServerPacket<SSendMessagePacket>
{ public static ushort StaticId => 1; }