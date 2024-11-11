using System.Collections.ObjectModel;
using System.Numerics;
using OpenTK.Mathematics;

namespace VoxelThing.Game.Maths;

public static class MathUtil
{
    private static readonly Dictionary<int, ReadOnlyCollection<Vector3i>> SpherePointLists = [];

    public static T Threshold<T>(T x, T min, T max)
        where T : INumber<T>
        => T.Clamp((x - min) / (max - min), T.Zero, T.One);

    public static float TrilinearInterpolation(
        float c000, float c001, float c010, float c011,
        float c100, float c101, float c110, float c111,
        float x, float y, float z)
    {
        float c00 = float.Lerp(c000, c100, x);
        float c01 = float.Lerp(c001, c101, x);
        float c10 = float.Lerp(c010, c110, x);
        float c11 = float.Lerp(c011, c111, x);

        float c0 = float.Lerp(c00, c10, y);
        float c1 = float.Lerp(c01, c11, y);

        return float.Lerp(c0, c1, z);
    }

    public static int HexValue(char c) => (c | 32) % 39 - 9;

    public static T FloorMod<T>(T x, T y)
        where T : INumber<T>
    {
        T result = x % y;
        if (result < T.Zero)
            result += y;
        return result;
    }

    public static T SquareOut<T>(T x)
        where T : IFloatingPoint<T>
        => T.One - (T.One - x) * (T.One - x);

    public static ReadOnlyCollection<Vector3i> GetSpherePoints(int radius)
    {
        if (SpherePointLists.TryGetValue(radius, out var points))
            return points;

        List<Vector3i> mutablePoints = [];
        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        for (int z = -radius; z <= radius; z++)
            mutablePoints.Add(new Vector3i(x, y, z));

        points = new([.. mutablePoints.OrderBy(v => v.EuclideanLengthSquared)]);
        SpherePointLists[radius] = points;
        return points;
    }

    public static OpenTK.Mathematics.Vector4 UvWithExtents(float x, float y, float width, float height)
        => new(x, y, x + width, y + height);
    
    public static double AngleTo(this Vector2d a, Vector2d b)
        => Math.Atan2(a.Y * b.X - a.X * b.Y, a.X * b.X + a.Y * b.Y);

    public static double DistanceToSquared(this Vector3d a, Vector3d b)
        => (a - b).LengthSquared;
    
    public static double DistanceTo(this Vector3d a, Vector3d b)
        => (a - b).Length;
    
    public static double DistanceToFast(this Vector3d a, Vector3d b)
        => (a - b).LengthFast;
}
