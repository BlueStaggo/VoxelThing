using OpenTK.Mathematics;

namespace VoxelThing.Game.Maths;

public readonly record struct Aabb(
    double MinX, double MinY, double MinZ,
    double MaxX, double MaxY, double MaxZ)
{
    public double MidX => (MinX + MaxX) / 2.0;
    public double MidY => (MinY + MaxY) / 2.0;
    public double MidZ => (MinZ + MaxZ) / 2.0;

    public Vector3d Min => new(MinX, MinY, MinZ);
    public Vector3d Max => new(MaxX, MaxY, MaxZ);
    public Vector3d Mid => new(MidX, MidY, MidZ);

    public Aabb(Vector3d position, double radius, double height)
        : this(position.X - radius, position.Y, position.Z - radius, position.X + radius, position.Y + height, position.Z + radius) { }

    public static Aabb FromExtents(double x, double y, double z, double ex, double ey, double ez)
        => new(x, y, z, x + ex, y + ey, z + ez);
    
    public Aabb Offset(double x, double y, double z)
        => new(MinX + x, MinY + y, MinZ + z, MaxX + x, MaxY + y, MaxZ + z);

    public Aabb Offset(Vector3d position)
        => Offset(position.X, position.Y, position.Z);

    public Aabb ExpandToPoint(double x, double y, double z)
        => new(x < 0.0 ? MinX + x : MinX, y < 0.0 ? MinY + y : MinY, z < 0.0 ? MinZ + z : MinZ,
            x > 0.0 ? MaxX + x : MaxX, y > 0.0 ? MaxY + y : MaxY, z > 0.0 ? MaxZ + z : MaxZ);

    public Aabb ExpandToPoint(Vector3d position)
        => ExpandToPoint(position.X, position.Y, position.Z);

    public double CalculateXOffset(Aabb other, double offset)
    {
        if (other.MaxY <= MinY || other.MinY >= MaxY
                || other.MaxZ <= MinZ || other.MinZ >= MaxZ)
            return offset;

        if (offset > 0.0 && other.MaxX <= MinX && MinX - other.MaxX < offset)
            offset = MinX - other.MaxX;

        if (offset < 0.0 && other.MinX >= MaxX && MaxX - other.MinX > offset)
            offset = MaxX - other.MinX;

        return offset;
    }

    public double CalculateYOffset(Aabb other, double offset)
    {
        if (other.MaxX <= MinX || other.MinX >= MaxX
                || other.MaxZ <= MinZ || other.MinZ >= MaxZ)
            return offset;

        if (offset > 0.0 && other.MaxY <= MinY && MinY - other.MaxY < offset)
            offset = MinY - other.MaxY;

        if (offset < 0.0 && other.MinY >= MaxY && MaxY - other.MinY > offset)
            offset = MaxY - other.MinY;

        return offset;
    }

    public double CalculateZOffset(Aabb other, double offset)
    {
        if (other.MaxX <= MinX || other.MinX >= MaxX
                || other.MaxY <= MinY || other.MinY >= MaxY)
            return offset;

        if (offset > 0.0 && other.MaxZ <= MinZ && MinZ - other.MaxZ < offset)
            offset = MinZ - other.MaxZ;

        if (offset < 0.0 && other.MinZ >= MaxZ && MaxZ - other.MinZ > offset)
            offset = MaxZ - other.MinZ;

        return offset;
    }

    public bool Intersects(Aabb other)
        => other.MaxX > MinX && other.MinX < MaxX
        && other.MaxY > MinY && other.MinY < MaxY
        && other.MaxZ > MinZ && other.MinZ < MaxZ;

    public bool Contains(Vector3d position) => Contains(position.X, position.Y, position.Z);

    public bool Contains(double x, double y, double z)
        => x > MinX && x < MaxX
        && y > MinY && y < MaxY
        && z > MinZ && z < MaxZ;

    public Direction GetClosestFace(Vector3d position, Vector3d direction)
    {
        direction.Normalize();
        direction *= 0.01;

        while (Contains(position))
            position -= direction;

        Vector3d[] faces =
        [
            new(MidX, MidY, MinZ),
            new(MidX, MidY, MaxZ),
            new(MinX, MidY, MidZ),
            new(MaxX, MidY, MidZ),
            new(MidX, MinY, MidZ),
            new(MidX, MaxY, MidZ),
        ];

        Direction closestFace = 0;
        double closestDistance = faces[0].LengthSquared;

        for (int i = 1; i < faces.Length; i++)
        {
            double distance = faces[i].LengthSquared;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFace = (Direction)i;
            }
        }

        return closestFace;
    }
}
