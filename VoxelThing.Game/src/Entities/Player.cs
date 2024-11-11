using OpenTK.Mathematics;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Entities;

public class Player : Entity
{
    public const double Acceleration = 0.2;
    public const double FlightAcceleration = 0.4;
    public const double Friction = 0.6;
    public const double JumpHeight = 0.5;

    private readonly IPlayerController controller;

    private readonly Interpolated<double> walkAmount = new();
    private readonly Interpolated<double> renderWalk = new();
    private readonly Interpolated<double> walkDir = new();
    private readonly Interpolated<double> fallAmount = new();

    private int jumpTimer = 0;
    private readonly InterpolatedBool jumpPressed = new();

    public readonly InterpolatedReadonly<double> RenderWalk;
    public readonly InterpolatedReadonly<double> FallAmount;
    
    public Player(World world, IPlayerController controller) : base(world)
    {
        this.controller = controller;
        RenderWalk = new(renderWalk);
        FallAmount = new(fallAmount);
    }

    public void SetTexture(string texture) => Texture = texture;

    public void OnGameUpdate()
    {
        Yaw.JumpTo(Yaw + controller.MoveYaw);
        Pitch.JumpTo(Pitch + controller.MovePitch);
        Pitch.Value = double.Clamp(Pitch, -89.9, 89.9);
        jumpPressed.Value |= controller.DoJump;
    }

    protected override void UpdateMovement()
    {
        base.UpdateMovement();

        if (jumpTimer > 0)
            jumpTimer--;
        
        if (!HasGravity)
        {
            Velocity.Value.Y *= Friction;
            if (controller.DoJump)
                Velocity.Value.Y = JumpHeight;
            if (controller.DoCrouch)
                Velocity.Value.Y = -JumpHeight;
        }

        double yawRadians = double.DegreesToRadians(Yaw);
        double moveForward = controller.MoveForward;
        double moveStrafe = controller.MoveStrafe;

        double moveDistance = Math.Sqrt(moveForward * moveForward + moveStrafe * moveStrafe);
        if (moveDistance > 1.0)
        {
            moveForward /= moveDistance;
            moveStrafe /= moveDistance;
        }

        double acceleration = Acceleration;
        if (!HasGravity)
            acceleration = FlightAcceleration;
        
        moveForward *= acceleration;
        moveStrafe *= acceleration;

        Velocity.Value.X += moveForward * Math.Cos(yawRadians) + moveStrafe * Math.Cos(yawRadians + Math.PI / 2.0);
        Velocity.Value.Z += moveForward * Math.Sin(yawRadians) + moveStrafe * Math.Sin(yawRadians + Math.PI / 2.0);
        Velocity.Value.X *= Friction;
        Velocity.Value.Z *= Friction;

        if ((OnGround || NoClip) && HasGravity && controller.DoJump)
            Velocity.Value.Y = JumpHeight;
        
        // if (jumpPressed.JustBecameTrue)
        // {
        //     if (jumpTimer > 0)
        //     {
        //         HasGravity = !HasGravity;
        //         jumpTimer = 0;
        //     }
        //     else
        //     {
        //         jumpTimer = 10;
        //     }
        // }

        jumpPressed.Tick();
        jumpPressed.Value = false;
    }

    protected override void UpdateAnimation()
    {
        walkAmount.Tick();
        renderWalk.Tick();
        walkDir.Tick();
        fallAmount.Tick();

        double walkAdd = Math.Min(Velocity.Value.Xz.Length, 1.0);
        if (!OnGround)
            walkAdd /= 2.5;

        if (HasGravity)
        {
            walkAmount.Value += walkAdd * 1.5;
            renderWalk.Value = Math.Sin(walkAmount) * Math.Min(walkAdd * 10.0, 1.0);

            if (OnGround && walkAmount.Value - walkAmount.PreviousValue < 0.01)
                walkAmount.Value = 0.0;
        }
        else
        {
            walkAmount.Value = 0.0;
            renderWalk.Value = 0.0;
        }

        if (walkAdd > 0.1)
            walkDir.Value = double.RadiansToDegrees(Velocity.Value.Xz.AngleTo(-Vector2d.UnitX));
        
        if (OnGround) fallAmount.Value = 0.0;
        else fallAmount.Value += (Velocity.Value.Y - fallAmount.Value) * 0.5;
    }

    public override Vector3d GetRenderPosition(double partialTick)
        => base.GetRenderPosition(partialTick) + Vector3d.UnitY * Math.Abs(renderWalk / 3.0);

    // TODO: Unscuff this
    public override int GetSpriteAngle(double partialTick, double cameraYaw)
        => (int)(MathUtil.FloorMod(-walkDir.GetInterpolatedValue(partialTick) + cameraYaw + 22.5f, 360.0f) / 45.0f) % 8;

    public override int SpriteFrame
    {
        get
        {
            int frame = 1;
            double walk = renderWalk.Value;

            if (walk >= 0.5) frame++;
            else if (walk <= -0.5) frame--;

            return frame;
        }
    }
}