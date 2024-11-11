using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering.Vertices;

public readonly struct VertexType(VertexAttribPointerType type, int size, bool normalized = false)
{
    private static readonly Dictionary<VertexAttribPointerType, int> TypeToStride = new()
    {
        [VertexAttribPointerType.Byte] = 1,
        [VertexAttribPointerType.UnsignedByte] = 1,
        [VertexAttribPointerType.Short] = 2,
        [VertexAttribPointerType.UnsignedShort] = 2,
        [VertexAttribPointerType.Int] = 4,
        [VertexAttribPointerType.UnsignedInt] = 4,
        [VertexAttribPointerType.Float] = 4,
        [VertexAttribPointerType.Double] = 8,
        [VertexAttribPointerType.HalfFloat] = 2,
    };

    public static readonly VertexType Vector2 = new(VertexAttribPointerType.Float, 2);
    public static readonly VertexType Vector3 = new(VertexAttribPointerType.Float, 3);
    public static readonly VertexType Color3 = new(VertexAttribPointerType.Float, 3, true);
    public static readonly VertexType Color3B = new(VertexAttribPointerType.UnsignedByte, 3, true);

    public readonly VertexAttribPointerType Type = type;
    public readonly int Size = size;
    public readonly bool Normalized = normalized;
    public readonly int Stride = size * TypeToStride[type];
}