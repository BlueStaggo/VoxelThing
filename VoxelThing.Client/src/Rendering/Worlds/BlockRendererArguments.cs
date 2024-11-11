using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Rendering.Worlds;

public readonly struct BlockRendererArguments
{
    public required MixedBindings OpaqueBindings { get; init; }
    public required MixedBindings TranslucentBindings { get; init; }
    public required IBlockAccess BlockAccess { get; init; }
    public required Chunk Chunk { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Z { get; init; }
}