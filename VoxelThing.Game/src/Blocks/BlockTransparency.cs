namespace VoxelThing.Game.Blocks;

[Flags]
public enum BlockTransparency
{
    None          = 0,
    DrawNeighbors = 0b0001,
    Transparent   = 0b0010,

    Opaque = None,
    Thick  = DrawNeighbors | Transparent,
    Thin   = Transparent,
}
