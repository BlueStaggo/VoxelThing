using OpenTK.Mathematics;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Entities;

public class ClientPlayer : Entity
{
    public const double Acceleration = 0.16;
    public const double AirAccelerationMultiplier = 0.2;
    public const double FlightAccelerationMultiplier = 2.0;
    public const double Friction = 0.6;
    public const double AirFriction = 0.9;
    public const double JumpHeight = 0.4;

    public override string? Type => null;
    protected override bool UpdateInRemoteWorld => true;

    private readonly IPlayerController controller;

    private readonly InterpolatedDouble walkAmount = new();
    private readonly InterpolatedDouble renderWalk = new();
    private readonly InterpolatedDouble walkDir = new(wrap: 360.0);
    private readonly InterpolatedDouble fallAmount = new();

    private int jumpTimer;
    private readonly InterpolatedBool jumpPressed = new();

    public readonly InterpolatedReadonly<double> RenderWalk;
    public readonly InterpolatedReadonly<double> FallAmount;
    public readonly InterpolatedDouble AnimationYaw = new(wrap: 360.0), AnimationPitch = new(wrap: 360.0);
    public readonly InterpolatedDouble LookYawOffset = new(), LookPitchOffset = new();

    public ClientPlayer(World world, IPlayerController controller) : base(world)
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
        Pitch.Value = Math.Clamp(Pitch, -89.9, 89.9);
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
            acceleration *= FlightAccelerationMultiplier;
        if (!OnGround)
            acceleration *= AirAccelerationMultiplier;
        
        moveForward *= acceleration;
        moveStrafe *= acceleration;

        Velocity.Value.X += moveForward * Math.Cos(yawRadians) + moveStrafe * Math.Cos(yawRadians + Math.PI / 2.0);
        Velocity.Value.Z += moveForward * Math.Sin(yawRadians) + moveStrafe * Math.Sin(yawRadians + Math.PI / 2.0);
        Velocity.Value.X *= OnGround ? Friction : AirFriction;
        Velocity.Value.Z *= OnGround ? Friction : AirFriction;

        if ((OnGround || NoClip) && HasGravity && controller.DoJump)
            Velocity.Value.Y = JumpHeight;
        
        if (jumpPressed.JustBecameTrue)
        {
            if (jumpTimer > 0)
            {
                HasGravity = !HasGravity;
                jumpTimer = 0;
            }
            else
            {
                jumpTimer = 5;
            }
        }

        jumpPressed.Tick();
        jumpPressed.Value = false;
    }

    protected override void UpdateAnimation()
    {
        walkAmount.Tick();
        renderWalk.Tick();
        walkDir.Tick();
        fallAmount.Tick();
        AnimationYaw.Tick();
        AnimationPitch.Tick();
        LookYawOffset.Tick();
        LookPitchOffset.Tick();

        AnimationYaw.Value = Yaw;
        AnimationPitch.Value = Pitch;

        double walkAdd = Math.Min(Velocity.Value.Xz.Length, 1.0);
        if (!OnGround)
                walkAdd /= 2.5;

        if (HasGravity)
        {
            walkAmount.Value += walkAdd * 1.5;
            renderWalk.Value = Math.Sin(walkAmount) * Math.Min(walkAdd * 15.0, 1.0);

            if (OnGround && walkAmount.Value - walkAmount.PreviousValue < 0.01)
                walkAmount.Value = 0.0;
        }
        else
        {
            walkAmount.Value = 0.0;
            renderWalk.Value = 0.0;
        }

        if (walkAdd > 0.05)
            walkDir.Value = double.RadiansToDegrees(Velocity.Value.Xz.AngleTo(-Vector2d.UnitX));
        
        if (OnGround) fallAmount.Value = 0.0;
        else fallAmount.Value += (Velocity.Value.Y - fallAmount.Value) * 0.5;

        LookYawOffset.Value *= 0.5;
        LookPitchOffset.Value *= 0.5;
        LookYawOffset.Value += AnimationYaw.Value - AnimationYaw.PreviousValue;
        LookPitchOffset.Value += AnimationPitch.Value - AnimationPitch.PreviousValue;
    }

    public override Vector3d GetRenderPosition(double partialTick)
        => base.GetRenderPosition(partialTick)
           + Vector3d.UnitY * Math.Abs(renderWalk.GetInterpolatedValue(partialTick) / 3.0);

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