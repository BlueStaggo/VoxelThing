using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering.Vertices;

public class VertexLayout(params VertexType[] vertexTypes)
{
    public static readonly VertexLayout Block = new(VertexType.Vector3, VertexType.Color3B, VertexType.Vector2);
    public static readonly VertexLayout World = new(VertexType.Vector3, VertexType.Color3, VertexType.Vector2);
    public static readonly VertexLayout Screen = new(VertexType.Vector2, VertexType.Color3, VertexType.Vector2);
    // TODO: Maybe make use of RGBA

    public readonly bool FloatOnly = vertexTypes.All(vt => vt.Type == VertexAttribPointerType.Float);
    public readonly int VertexSize = vertexTypes.Sum(vt => vt.Size);
    private readonly int totalStride = vertexTypes.Sum(vt => vt.Stride);

    public int GenBuffer(int vao)
    {
        int buffer = GL.GenBuffer();
        GL.BindVertexArray(vao);

        int size = 0;
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        for (int i = 0; i < vertexTypes.Length; i++)
        {
            VertexType vertexType = vertexTypes[i];
            GL.VertexAttribPointer(i, vertexType.Size, vertexType.Type, vertexType.Normalized, totalStride, size);
            GL.EnableVertexAttribArray(i);
            size += vertexType.Stride;
        }

        return buffer;
    }

    public static void BufferData<T>(int vbo, int length, T[] data, bool dynamic)
        where T : struct
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, length, data, dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
    }
}