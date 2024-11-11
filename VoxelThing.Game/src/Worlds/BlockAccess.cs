using VoxelThing.Game.Blocks;

namespace VoxelThing.Game.Worlds;

public interface IBlockAccess
{
    public Block? GetBlock(int x, int y, int z);
    public bool IsAir(int x, int y, int z) => GetBlock(x, y, z) is null;
}