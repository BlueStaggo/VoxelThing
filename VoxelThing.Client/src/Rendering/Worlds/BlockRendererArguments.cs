using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Client.Rendering.Worlds;

public struct BlockRendererArguments
{
    public required MixedBindings OpaqueBindings;
    public required MixedBindings TranslucentBindings;
    public required IBlockAccess BlockAccess;
    public int X;
    public int Y;
    public int Z;
}