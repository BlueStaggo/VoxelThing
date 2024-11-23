using OpenTK.Mathematics;
using PDS;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Game.Entities;

public class Entity(World world) : IStructureItemSerializable
{
    public const double Gravity = 0.08;
    public const double TerminalVelocity = 4.0;

    public readonly World World = world;

    public string Texture { get; protected set; } = "template";

    public double Radius { get; protected set; } = 0.8;
    public double Height { get; protected set; } = 1.8;
    public Aabb CollisionBox => new(Position, Radius, Height);

    public readonly InterpolatedVector3d Position = new();
    public readonly InterpolatedVector3d Velocity = new();
    public readonly InterpolatedDouble Yaw = new(wrap: 360.0), Pitch = new(wrap: 360.0);

    public Vector3i BlockPosition => new(
        (int)Math.Floor(Position.Value.X),
        (int)Math.Floor(Position.Value.Y),
        (int)Math.Floor(Position.Value.Z)
    );

    public virtual Vector2 SpriteSize => new(2.0f, 2.0f);
    public virtual int SpriteFrame => 0;
    public virtual double EyeLevel => Height - 0.2;

    public readonly InterpolatedBool OnGround = new();
    public bool NoClip = false;
    public bool HasGravity = true;

    public void Tick()
    {
        Position.Tick();
        Velocity.Tick();
        Yaw.Tick();
        Pitch.Tick();
        OnGround.Tick();

        Update();
    }

    protected virtual void Update()
    {
        UpdateMovement();
        UpdateAnimation();
        UpdateCollision();
    }

    protected virtual void UpdateMovement()
    {
        if (HasGravity && Velocity.Value.Y > -TerminalVelocity)
            Velocity.Value.Y = Math.Max(Velocity.Value.Y - Gravity, -TerminalVelocity);
    }

    protected virtual void UpdateAnimation() { }

    protected void UpdateCollision()
    {
        if (NoClip)
        {
            OnGround.Value = false;
            Position.Value += Velocity.Value;
            return;
        }

        Aabb collisionBox = CollisionBox;
        List<Aabb> intersectingBoxes = World.GetSurroundingCollision(collisionBox.ExpandToPoint(Velocity.Value));

        double oldVelocityY = Velocity.Value.Y;

        foreach (Aabb box in intersectingBoxes)
            Velocity.Value.Y = box.CalculateYOffset(collisionBox, Velocity.Value.Y);
        collisionBox = collisionBox.Offset(0.0, Velocity.Value.Y, 0.0);

        foreach (Aabb box in intersectingBoxes)
            Velocity.Value.X = box.CalculateXOffset(collisionBox, Velocity.Value.X);
        collisionBox = collisionBox.Offset(Velocity.Value.X, 0.0, 0.0);

        foreach (Aabb box in intersectingBoxes)
            Velocity.Value.Z = box.CalculateZOffset(collisionBox, Velocity.Value.Z);
        collisionBox = collisionBox.Offset(0.0, 0.0, Velocity.Value.Z);

        OnGround.Value = oldVelocityY < 0.0 && oldVelocityY < Velocity.Value.Y;
        Position.Value += Velocity.Value;
    }

    public virtual Vector3d GetRenderPosition(double partialTick) => Position.GetInterpolatedValue(partialTick);

    public virtual double GetRenderYaw(double partialTick) => Yaw.GetInterpolatedValue(partialTick);

    public virtual double GetRenderPitch(double partialTick) => Pitch.GetInterpolatedValue(partialTick);

    public virtual int GetSpriteAngle(double partialTick, double cameraYaw)
        => (int)(MathUtil.FloorMod(GetRenderYaw(partialTick) - 180.0f - cameraYaw + 22.5f, 360.0f) / 45.0f) % 8;

    public StructureItem Serialize() => new CompoundItem()
        .Put("PosX", Position.Value.X)
        .Put("PosY", Position.Value.Y)
        .Put("PosZ", Position.Value.Z)
        .Put("VelX", Velocity.Value.X)
        .Put("VelY", Velocity.Value.Y)
        .Put("VelZ", Velocity.Value.Z)
        .Put("RotYaw", Yaw.Value)
        .Put("RotPitch", Pitch.Value);

    public void Deserialize(CompoundItem compound)
    {
        Position.JumpTo(new(
            compound["PosX"]?.DoubleValue ?? 0.0,
            compound["PosY"]?.DoubleValue ?? 0.0,
            compound["PosZ"]?.DoubleValue ?? 0.0
        ));
        Velocity.JumpTo(new(
            compound["VelX"]?.DoubleValue ?? 0.0,
            compound["VelY"]?.DoubleValue ?? 0.0,
            compound["VelZ"]?.DoubleValue ?? 0.0
        ));
        Yaw.JumpTo(compound["RotYaw"]?.DoubleValue ?? 0.0);
        Pitch.JumpTo(compound["RotPitch"]?.DoubleValue ?? 0.0);
    }
}