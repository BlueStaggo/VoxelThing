using OpenTK.Mathematics;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Blocks.Texture;

public interface IBlockTexture
{
    public Vector2i Get(Direction face, IBlockAccess? blockAccess, int x, int y, int z);
    public Vector2i Get(Direction face) => Get(face, null, 0, 0, 0);
}

public readonly record struct AllSidesTexture(Vector2i Coord) : IBlockTexture
{
    public AllSidesTexture(int x, int y) : this(new(x, y)) { }

    public Vector2i Get(Direction face, IBlockAccess? blockAccess, int x, int y, int z) => Coord;
}

public readonly record struct ColumnTexture(Vector2i Side, Vector2i TopBottom) : IBlockTexture
{
    public Vector2i Get(Direction face, IBlockAccess? blockAccess, int x, int y, int z)
    => face switch
    {
        Direction.Up or Direction.Down => TopBottom,
        _ => Side,
    };
}

public readonly record struct SideTopBottomTexture(Vector2i Side, Vector2i Top, Vector2i Bottom) : IBlockTexture
{
    public Vector2i Get(Direction face, IBlockAccess? blockAccess, int x, int y, int z)
    => face switch
    {
        Direction.Up => Top,
        Direction.Down => Bottom,
        _ => Side,
    };
}

public readonly record struct GrassTexture(Vector2i Side, Vector2i Top, Vector2i Bottom) : IBlockTexture
{
    public Vector2i Get(Direction face, IBlockAccess? blockAccess, int x, int y, int z)
    => face switch
    {
        Direction.Up => Top,
        Direction.Down => Bottom,
        _ => blockAccess?.GetBlock(x + face.GetX(), y + face.GetY() - 1, z + face.GetZ()) == Block.Grass ? Top : Side,
    };
}