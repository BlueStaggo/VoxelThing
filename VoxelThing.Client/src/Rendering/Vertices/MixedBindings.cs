using System.Text;
using OpenTK.Graphics.OpenGL;

namespace VoxelThing.Client.Rendering.Vertices;

public class MixedBindings : Bindings
{
    protected override int DataSize => (int) dataStream.Length;

    private MemoryStream dataStream = new();
    private BinaryWriter binaryWriter;

    public MixedBindings(VertexLayout layout, PrimitiveType primitiveType = PrimitiveType.Triangles)
        : base(layout, primitiveType)
    {
        binaryWriter = new(dataStream, Encoding.UTF8, true);
    }

    public MixedBindings Put(byte vertex)
    {
        binaryWriter.Write(vertex);
        return this;
    }

    public MixedBindings Put(short vertex)
    {
        binaryWriter.Write(vertex);
        return this;
    }

    public MixedBindings Put(int vertex)
    {
        binaryWriter.Write(vertex);
        return this;
    }

    public MixedBindings Put(float vertex)
    {
        binaryWriter.Write(vertex);
        return this;
    }

    public MixedBindings Put(params byte[] vertices)
    {
        binaryWriter.Write(vertices);
        return this;
    }

    protected override void UploadVertices(bool dynamic)
        => VertexLayout.BufferData(Vbo, (int)dataStream.Length, dataStream.GetBuffer(), dynamic);

    public override void Clear()
    {
        if (dataStream.Length > 1024)
        {
            dataStream.Dispose();
            binaryWriter.Dispose();
            dataStream = new();
            binaryWriter = new(dataStream, Encoding.UTF8, true);
        }
        else
        {
            dataStream.Position = 0;
            dataStream.SetLength(0);
        }

        base.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        binaryWriter.Dispose();
        base.Dispose(disposing);
    }
}