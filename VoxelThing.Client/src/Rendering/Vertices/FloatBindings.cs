using VoxelThing.Game;

namespace VoxelThing.Client.Rendering.Vertices;

public class FloatBindings : Bindings
{
    protected override int DataSize => nextData.Count;

    private readonly List<float> nextData = [];

    public FloatBindings(VertexLayout layout) : base(layout)
    {
        if (!layout.FloatOnly)
            throw new ArgumentException("FloatBindings only accepts vertex layouts consisting entirely of float data", nameof(layout));
    }

    public FloatBindings(params VertexType[] types) : this(new VertexLayout(types)) { }

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
        => VertexLayout.BufferData(Vbo, nextData.Count * 4, nextData.GetInternalArray(), dynamic);

    public override void Clear()
    {
        nextData.Clear();
        base.Clear();
    }
}