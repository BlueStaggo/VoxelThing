using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using VoxelThing.Game;

namespace VoxelThing.Client.Rendering.Vertices;

public class FloatBindings : Bindings
{
    protected override int DataSize => nextData.Count;

    private readonly List<float> nextData = [];

    public FloatBindings(VertexLayout layout, PrimitiveType primitiveType = PrimitiveType.Triangles)
        : base(layout, primitiveType)
    {
        if (!layout.FloatOnly)
            throw new ArgumentException("FloatBindings only accepts vertex layouts consisting entirely of float data", nameof(layout));
    }

    public FloatBindings Put(float vertex)
    {
        nextData.Add(vertex);
        return this;
    }

    public FloatBindings Put(params float[] vertices)
    {
        nextData.AddRange(vertices);
        return this;
    }

    protected override void UploadVertices(bool dynamic)
    {
        if (RuntimeFeature.IsDynamicCodeCompiled)
            VertexLayout.BufferData(Vbo, nextData.Count * sizeof(float), nextData.GetInternalArray(), dynamic);   
        else
            unsafe
            {
                fixed (float* data = CollectionsMarshal.AsSpan(nextData))
                    VertexLayout.BufferData(Vbo, nextData.Count * sizeof(float), (IntPtr)data, dynamic);   
            }
    }

    public override void Clear()
    {
        nextData.Clear();
        base.Clear();
    }
}