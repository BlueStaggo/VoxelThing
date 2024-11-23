using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace VoxelThing.Client.Rendering.Shaders;

public abstract class Uniform<T>
{
    public readonly int Location;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "It doesn't really matter")]
    protected Uniform(int handle, string name, bool @abstract = false)
    {
        Location = GL.GetUniformLocation(handle, name);
        if (Location == -1 && !@abstract)
        {
            string caller = "???";
            StackTrace stackTrace = new();
            for (int i = 0; i < 10; i++)
            {
                Type? declaringType = stackTrace.GetFrame(i + 1)?.GetMethod()?.DeclaringType;
                if (!(declaringType?.IsAssignableTo(typeof(Shader)) ?? false)) continue;
                
                caller = declaringType.Name;
                break;
            }
            
            Console.Error.WriteLine(
                $"Uniform \"{name}\" not found in shader program {handle} of {caller}", nameof(name));
        }
    }

    public abstract void Set(T value);
}

public abstract class Uniform2<T, TC>(int handle, string name, bool @abstract = false)
    : Uniform<T>(handle, name, @abstract)
{
    public abstract void Set(TC x, TC y);
}

public abstract class Uniform3<T, TC>(int handle, string name, bool @abstract = false)
    : Uniform<T>(handle, name, @abstract)
{
    public abstract void Set(TC x, TC y, TC z);
}

public abstract class Uniform4<T, TC>(int handle, string name, bool @abstract = false)
    : Uniform<T>(handle, name, @abstract)
{
    public abstract void Set(TC x, TC y, TC z, TC w);
}

public class BoolUniform(int handle, string name, bool @abstract = false)
    : Uniform<bool>(handle, name, @abstract)
{
    public override void Set(bool value) => GL.Uniform1(Location, value ? 1 : 0);
}

public class IntUniform(int handle, string name, bool @abstract = false)
    : Uniform<int>(handle, name, @abstract)
{
    public override void Set(int value) => GL.Uniform1(Location, value);
}

public class UIntUniform(int handle, string name, bool @abstract = false)
    : Uniform<uint>(handle, name, @abstract)
{
    public override void Set(uint value) => GL.Uniform1(Location, value);
}

public class FloatUniform(int handle, string name, bool @abstract = false)
    : Uniform<float>(handle, name, @abstract)
{
    public override void Set(float value) => GL.Uniform1(Location, value);
}

public class DoubleUniform(int handle, string name, bool @abstract = false)
    : Uniform<double>(handle, name, @abstract)
{
    public override void Set(double value) => GL.Uniform1(Location, value);
}

public class Vector2Uniform(int handle, string name, bool @abstract = false)
    : Uniform2<Vector2, float>(handle, name, @abstract)
{
    public override void Set(Vector2 value) => Set(value.X, value.Y);
    public override void Set(float x, float y) => GL.Uniform2(Location, x, y);
}

public class Vector3Uniform(int handle, string name, bool @abstract = false)
    : Uniform3<Vector3, float>(handle, name, @abstract)
{
    public override void Set(Vector3 value) => Set(value.X, value.Y, value.Z);
    public override void Set(float x, float y, float z) => GL.Uniform3(Location, x, y, z);
    
    public void SetRgb(int r, int g, int b) => Set(r / 255.0f, g / 255.0f, b / 255.0f);

    public void SetRgb(int rgb) => Set(
        ((rgb >> 16) & 255) / 255.0f,
        ((rgb >> 8) & 255) / 255.0f,
        (rgb & 255) / 255.0f
    );
}

public class Vector4Uniform(int handle, string name, bool @abstract = false)
    : Uniform4<Vector4, float>(handle, name, @abstract)
{
    public override void Set(Vector4 value) => Set(value.X, value.Y, value.Z, value.W);
    public override void Set(float x, float y, float z, float w) => GL.Uniform4(Location, x, y, z, w);
    
    public void Set(Color4 value) => Set(value.R, value.G, value.B, value.A);
    public void SetRgba(int r, int g, int b, int a) => Set(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    public void SetRgb(int r, int g, int b) => Set(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
    public void SetRgb(float r, float g, float b) => Set(r, g, b, 1.0f);
    
    public void SetRgba(uint rgba) => Set(
        ((rgba >> 24) & 255) / 255.0f,
        ((rgba >> 16) & 255) / 255.0f,
        ((rgba >> 8) & 255) / 255.0f,
        (rgba & 255) / 255.0f
    );
    
    public void SetRgb(int rgb) => Set(
        ((rgb >> 16) & 255) / 255.0f,
        ((rgb >> 8) & 255) / 255.0f,
        (rgb & 255) / 255.0f,
        1.0f
    );
}

public class Matrix4Uniform(int handle, string name, bool @abstract = false)
    : Uniform<Matrix4>(handle, name, @abstract)
{
    public override void Set(Matrix4 value) => GL.UniformMatrix4(Location, false, ref value);
}
