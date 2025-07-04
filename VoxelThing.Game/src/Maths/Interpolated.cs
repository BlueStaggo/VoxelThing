using OpenTK.Mathematics;

namespace VoxelThing.Game.Maths;

public abstract class Interpolated<T>
{
    public T PreviousValue { get; protected set; }
    public T Value;
    public bool HasChanged => Equals(PreviousValue, Value);

    protected Interpolated(T value) => PreviousValue = Value = value;

    public virtual void Tick() => PreviousValue = Value;
    public void JumpTo(T value) => PreviousValue = Value = value;
    public abstract T GetInterpolatedValue(double t);

    public static implicit operator T(Interpolated<T> p) => p.Value;
}

public class InterpolatedDouble(double value = 0.0, double wrap = 0.0) : Interpolated<double>(value)
{
    public override double GetInterpolatedValue(double t)
    {
        if (wrap <= 0.0)
            return double.Lerp(PreviousValue, Value, t);

        double a = MathUtil.FloorMod(PreviousValue, wrap);
        double b = MathUtil.FloorMod(Value, wrap);
        if (MathUtil.FloorMod(b - a, wrap) < MathUtil.FloorMod(a - b, wrap))
            while (b < a)
                b += wrap;
        else
            while (b > a)
                b -= wrap;
        return MathUtil.FloorMod(double.Lerp(a, b, t), wrap);
    }
}

public class InterpolatedVector3d(Vector3d value) : Interpolated<Vector3d>(value)
{
    public InterpolatedVector3d() : this(Vector3d.Zero) { }
    public override Vector3d GetInterpolatedValue(double t) => Vector3d.Lerp(PreviousValue, Value, t);
}

public class InterpolatedBool(bool value = false) : Interpolated<bool>(value)
{
    public bool JustBecameTrue => Value && !PreviousValue;
    public bool JustBecameFalse => !Value && PreviousValue;
    
    public override bool GetInterpolatedValue(double t) => Value;
}

public class InterpolatedReadonly<T>(Interpolated<T> interpolated)
{
    public T PreviousValue => interpolated.PreviousValue;
    public T Value => interpolated.Value;

    public T GetInterpolatedValue(double t) => interpolated.GetInterpolatedValue(t);

    public static implicit operator T(InterpolatedReadonly<T> p) => p.Value;
}
