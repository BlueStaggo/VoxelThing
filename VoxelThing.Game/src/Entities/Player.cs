using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Entities;

public class Player(World world) : Entity(world)
{
    public override string Type => "player";
}