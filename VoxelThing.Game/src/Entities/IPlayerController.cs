namespace VoxelThing.Game.Entities;

public interface IPlayerController
{
    public double MoveForward { get; }
    public double MoveStrafe { get; }
    public double MoveYaw { get; }
    public double MovePitch { get; }
    public bool DoJump { get; }
    public bool DoCrouch { get; }
}