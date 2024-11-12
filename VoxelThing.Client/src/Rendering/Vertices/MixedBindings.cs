using System.Text;

namespace VoxelThing.Client.Rendering.Vertices;

public class MixedBindings : Bindings
{
    protected override int DataSize => dataSize;

    private MemoryStream dataStream = new();
    private BinaryWriter binaryWriter;
    private int dataSize;

    public MixedBindings(params VertexType[] types) : this(new VertexLayout(types)) { }

    public MixedBindings(VertexLayout layout) : base(layout)
    {
        binaryWriter = new(dataStream, Encoding.UTF8, true);
    }

    public MixedBindings Put(byte vertex)
    {
        binaryWriter.Write(vertex);
        dataSize++;
        return this;
    }

    public MixedBindings Put(short vertex)
    {
        binaryWriter.Write(vertex);
        dataSize += sizeof(short);
        return this;
    }

    public MixedBindings Put(int vertex)
    {
        binaryWriter.Write(vertex);
        dataSize += sizeof(int);
        return this;
    }

    public MixedBindings Put(float vertex)
    {
        binaryWriter.Write(vertex);
        dataSize += sizeof(float);
        return this;
    }

    public MixedBindings Put(params byte[] vertices)
    {
        binaryWriter.Write(vertices);
        dataSize += vertices.Length;
        return this;
    }

    protected override void UploadVertices(bool dynamic)
        => VertexLayout.BufferData(Vbo, (int)dataStream.Length, dataStream.GetBuffer(), dynamic);

    public override void Clear()
    {
        if (dataStream.Length > 256)
        {
            dataStream = new();
            binaryWriter.Dispose();
            binaryWriter = new(dataStream, Encoding.UTF8, true);
        }
        else
        {
            dataStream.Position = 0;
            dataStream.SetLength(0);
        }

        dataSize = 0;
        
        base.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        binaryWriter.Dispose();
        base.Dispose(disposing);
    }
}