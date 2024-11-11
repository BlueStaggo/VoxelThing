using System.Text;

namespace VoxelThing.Client.Rendering.Vertices;

public class MixedBindings(VertexLayout layout) : Bindings(layout)
{
    private MemoryStream dataStream = new();
    private BinaryWriter? binaryWriter;

    public MixedBindings(params VertexType[] types) : this(new VertexLayout(types)) { }

    private BinaryWriter GetBinaryWriter() => binaryWriter ??= new(dataStream, Encoding.UTF8, true);

    public MixedBindings Put(byte vertex)
    {
        GetBinaryWriter().Write(vertex);
        CoordinateCount++;
        return this;
    }

    public MixedBindings Put(short vertex)
    {
        GetBinaryWriter().Write(vertex);
        CoordinateCount++;
        return this;
    }

    public MixedBindings Put(int vertex)
    {
        GetBinaryWriter().Write(vertex);
        CoordinateCount++;
        return this;
    }

    public MixedBindings Put(float vertex)
    {
        GetBinaryWriter().Write(vertex);
        CoordinateCount++;
        return this;
    }

    public MixedBindings Put(params byte[] vertices)
    {
        GetBinaryWriter().Write(vertices);
        CoordinateCount += vertices.Length;
        return this;
    }

    protected override void UploadVertices(bool dynamic)
        => VertexLayout.BufferData(Vbo, (int)dataStream.Length, dataStream.GetBuffer(), dynamic);

    public override void Clear()
    {
        if (dataStream.Length > 256)
        {
            dataStream = new();
        }
        else
        {
            dataStream.Position = 0;
            dataStream.SetLength(0);
        }

        binaryWriter?.Dispose();
        binaryWriter = null;

        base.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        binaryWriter?.Dispose();
        binaryWriter = null;
        base.Dispose(disposing);
    }
}