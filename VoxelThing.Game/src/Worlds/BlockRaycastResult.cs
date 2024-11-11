using VoxelThing.Game.Blocks;

namespace VoxelThing.Game.Worlds;

public readonly struct BlockRaycastResult
{
    public static readonly BlockRaycastResult NoHit = new(false, 0, 0, 0, (Direction)0);

    public readonly bool Hit;
    public readonly int HitX;
    public readonly int HitY;
    public readonly int HitZ;
    public readonly Direction HitFace;

    public BlockRaycastResult(int hitX, int hitY, int hitZ, Direction hitFace)
        : this(true, hitX, hitY, hitZ, hitFace) { }

    private BlockRaycastResult(bool hit, int hitX, int hitY, int hitZ, Direction hitFace)
    {
        Hit = hit;
        HitX = hitX;
        HitY = hitY;
        HitZ = hitZ;
        HitFace = hitFace;
    }

    public string GetDebugText(World world)
    {
        if (!Hit)
            return Block.AirId.FullName;

        Block? block = world.GetBlock(HitX, HitY, HitZ);
        return (block?.Id ?? Block.AirId).FullName;
    }
}