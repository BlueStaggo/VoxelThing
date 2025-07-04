using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Entities;

public class BouncyEntity(World world) : Entity(world)
{
    public const double Drag = 0.99;
    public const double BounceFactor = 0.9;

    public override string? Type => "bouncy";

    protected override void UpdateMovement()
    {
        base.UpdateMovement();

        if (OnWallX)
            Velocity.Value.X = Velocity.PreviousValue.X * -BounceFactor;
        
        if (OnWallZ)
            Velocity.Value.Z = Velocity.PreviousValue.Z * -BounceFactor;
        
        if (OnGround || OnCeiling)
            Velocity.Value.Y = Velocity.PreviousValue.Y * -BounceFactor;
        
        Velocity.Value.X *= Drag;
        Velocity.Value.Z *= Drag;
    }
}