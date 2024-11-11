using System.Numerics;
using OpenTK.Mathematics;

namespace VoxelThing.Game.Maths;

public class Interpolated<T>
    where T : IFloatingPointIeee754<T>
{
    public T PreviousValue { get; private set; }
    public T Value;

    public Interpolated() : this(T.Zero) { }
    public Interpolated(T value) => PreviousValue = Value = value;

    public void Tick() => PreviousValue = Value;
    public void JumpTo(T value) => PreviousValue = Value = value;
    public T GetInterpolatedValue(double t) => T.Lerp(PreviousValue, Value, T.CreateTruncating(t));

    public static implicit operator T(Interpolated<T> p) => p.Value;
}

public class InterpolatedReadonly<T>(Interpolated<T> interpolated)
    where T : IFloatingPointIeee754<T>
{
    public T PreviousValue => interpolated.PreviousValue;
    public T Value => interpolated.Value;

    public T GetInterpolatedValue(double t) => interpolated.GetInterpolatedValue(t);

    public static implicit operator T(InterpolatedReadonly<T> p) => p.Value;
}

public class InterpolatedVector3d
{
    public Vector3d PreviousValue { get; private set; }
    public Vector3d Value;

    public InterpolatedVector3d() : this(Vector3d.Zero) { }
    public InterpolatedVector3d(Vector3d value) => PreviousValue = Value = value;

    public void Tick() => PreviousValue = Value;
    public void JumpTo(Vector3d value) => PreviousValue = Value = value;
    public Vector3d GetInterpolatedValue(double t) => Vector3d.Lerp(PreviousValue, Value, t);

    public static implicit operator Vector3d(InterpolatedVector3d p) => p.Value;
}

public class InterpolatedBool
{
    public bool PreviousValue { get; private set; }
    public bool Value;

    public bool JustBecameTrue => Value && !PreviousValue;
    public bool JustBecameFalse => !Value && PreviousValue;

    public InterpolatedBool() : this(false) { }
    public InterpolatedBool(bool value) => PreviousValue = Value = value;

    public void Tick() => PreviousValue = Value;
    public void JumpTo(bool value) => PreviousValue = Value = value;
    
    public static implicit operator bool(InterpolatedBool p) => p.Value;
}
