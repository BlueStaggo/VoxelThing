using OpenTK.Mathematics;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Rendering.Worlds;

public readonly struct Frustum
{
    private readonly Plane top, bottom, right, left, far, near;

    public Frustum(Camera camera)
    {
        float halfVSide = camera.Far * MathF.Tan(camera.FovRadians / 2.0f);
        float halfHSide = halfVSide * camera.Aspect;
        Vector3 frontNear = camera.Front * camera.Near;
        Vector3 frontFar = camera.Front * camera.Far;

        near = new(frontNear, camera.Front);
        far = new(frontFar, camera.Front);
        right = new(Vector3.Zero, Vector3.Cross(frontFar - camera.Right * halfHSide, camera.Up));
        left = new(Vector3.Zero, Vector3.Cross(camera.Up, frontFar + camera.Right * halfHSide));
        top = new(Vector3.Zero, Vector3.Cross(camera.Right, frontFar - camera.Up * halfVSide));
        bottom = new(Vector3.Zero, Vector3.Cross(frontFar + camera.Up * halfVSide, camera.Right));
    }

    public bool TestAabb(Aabb aabb)
    {
        Vector3 center = (Vector3)aabb.Mid;
        Vector3 extents = (Vector3)(aabb.Max - aabb.Mid);

        // return top.IntersectsAabb(center, extents)
        //     && bottom.IntersectsAabb(center, extents)
        //     && right.IntersectsAabb(center, extents)
        //     && left.IntersectsAabb(center, extents)
        //     && far.IntersectsAabb(center, extents)
        //     && near.IntersectsAabb(center, extents);
        return true;
    }
}

public readonly struct Plane(Vector3 pos, Vector3 normal)
{
    public readonly Vector3 Normal = normal.Normalized();
    public readonly float Distance = Vector3.Dot(pos, normal);

    public float GetSignedDistance(Vector3 point) => Vector3.Dot(Normal, point) - Distance;

    public bool IntersectsAabb(Vector3 center, Vector3 extents)
    {
        float radius =
            extents.X * MathF.Abs(Normal.X)
            + extents.Y * MathF.Abs(Normal.Y)
            + extents.Z * MathF.Abs(Normal.Z);
        return MathF.Abs(GetSignedDistance(center)) <= radius;
    }
}
