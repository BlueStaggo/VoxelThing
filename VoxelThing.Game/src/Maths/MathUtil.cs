using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Numerics;
using OpenTK.Mathematics;

namespace VoxelThing.Game.Maths;

public static class MathUtil
{
    private static readonly ConcurrentDictionary<Vector3i, ReadOnlyCollection<Vector3i>> CuboidPointLists = [];

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

    public static void TrilinearInterpolation<T>(T[,,] src, T[,,] dst, int rx, int ry, int rz)
        where T : IFloatingPoint<T>
    {
        int dw = dst.GetLength(0) / rx;
        int dh = dst.GetLength(1) / ry;
        int dl = dst.GetLength(2) / rz;
        
        for (int x = 0; x < dw; x++)
        for (int y = 0; y < dh; y++)
        for (int z = 0; z < dl; z++)
        {
            T n000 = src[x, y, z];
            T n001 = src[x, y, z + 1];
            T n010 = src[x, y + 1, z];
            T n011 = src[x, y + 1, z + 1];
            T nL00 = src[x + 1, y, z] - n000;
            T nL01 = src[x + 1, y, z + 1] - n001;
            T nL10 = src[x + 1, y + 1, z] - n010;
            T nL11 = src[x + 1, y + 1, z + 1] - n011;

            for (int lx = 0; lx < rx; lx++)
            {
                T lxlerp = T.CreateTruncating(lx) / T.CreateTruncating(rx);
                T n00 = n000 + nL00 * lxlerp;
                T n01 = n001 + nL01 * lxlerp;
                T nL0 = n010 + nL10 * lxlerp - n00;
                T nL1 = n011 + nL11 * lxlerp - n01;

                for (int ly = 0; ly < ry; ly++)
                {
                    T lylerp = T.CreateTruncating(ly) / T.CreateTruncating(ry);
                    T n0 = n00 + nL0 * lylerp;
                    T nL = n01 + nL1 * lylerp - n0;

                    for (int lz = 0; lz < rz; lz++)
                    {
                        T lzlerp = T.CreateTruncating(lz) / T.CreateTruncating(rz);
                        T n = n0 + nL * lzlerp;
                        dst[x * rx + lx, y * ry + ly, z * rz + lz] = n;
                    }
                }
            }
        }
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

    public static ReadOnlyCollection<Vector3i> GetCuboidPoints(int width, int height, int length)
    {
        Vector3i dimensions = (width, height, length);
        
        if (CuboidPointLists.TryGetValue(dimensions, out var points))
            return points;

        List<Vector3i> mutablePoints = [];
        for (int x = -width; x <= width; x++)
        for (int y = -height; y <= height; y++)
        for (int z = -length; z <= length; z++)
            mutablePoints.Add(new Vector3i(x, y, z));

        points = new([.. mutablePoints.OrderBy(v => v.EuclideanLengthSquared)]);
        CuboidPointLists[dimensions] = points;
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
