using OpenTK.Graphics.OpenGL;
using VoxelThing.Game;

namespace VoxelThing.Client.Rendering.Vertices;

public abstract class Bindings(VertexLayout layout, PrimitiveType primitiveType = PrimitiveType.Triangles) : IDisposable
{
    public bool IsEmpty => DataSize == 0;
    public PrimitiveType PrimitiveType = primitiveType;
    
	protected readonly VertexLayout Layout = layout;
	protected abstract int DataSize { get; }
    protected int Vbo { get; private set; }

    private int vao;
    private int verticesDrawn;
    
    private List<int>? nextIndices;
	private int ebo;
	private int indexCount;
	private int maxIndex;
    private bool disposed;
    
    ~Bindings() => Dispose(false);

    public void AddIndex(int index)
    {
        if (nextIndices is null)
        {
            nextIndices = [];
            ebo = GL.GenBuffer();
        }

        nextIndices.Add(index);
        if (index + 1 > maxIndex)
            maxIndex = index + 1;
    }

    public void AddIndices(params int[] indices)
    {
        int offset = maxIndex;
        for (int i = 0; i < indices.Length; i++)
            AddIndex(indices[i] + offset);
    }
    
    public void AddIndividualQuadIndices()
    {
        int offset = maxIndex;
        AddIndex(offset);
        AddIndex(offset + 1);
        AddIndex(offset + 2);
        AddIndex(offset + 2);
        AddIndex(offset + 3);
        AddIndex(offset);
    }

    public void AddQuadIndices() => AddIndices(0, 1, 2, 2, 3, 0);

    public void Upload(bool dynamic)
    {
        if (DataSize == 0)
            return;
        
        if (vao == 0)
        {
            vao = GL.GenVertexArray();
            Vbo = Layout.GenBuffer(vao);
        }

        GL.BindVertexArray(vao);
        UploadVertices(dynamic);
        UploadIndices(dynamic);
        Clear();
    }

    protected abstract void UploadVertices(bool dynamic);

    private void UploadIndices(bool dynamic)
    {
        if (DataSize == 0 || nextIndices is null || ebo == 0) return;

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, nextIndices.Count * sizeof(int), nextIndices.GetInternalArray(),
            dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);
    }

    public virtual void Clear()
    {
        indexCount = nextIndices?.Count ?? 0;
        verticesDrawn = indexCount > 0 ? indexCount : DataSize;
        maxIndex = 0;
        nextIndices?.Clear();
    }

    public void Draw()
    {
        GL.BindVertexArray(vao);
        if (indexCount > 0 && ebo != 0)
            GL.DrawElements(PrimitiveType, verticesDrawn, DrawElementsType.UnsignedInt, 0);
        else if (verticesDrawn > 0)
            GL.DrawArrays(PrimitiveType, 0, verticesDrawn);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (vao != 0) GL.DeleteVertexArray(vao);
        if (Vbo != 0) GL.DeleteBuffer(Vbo);
        if (ebo != 0) GL.DeleteBuffer(ebo);
        
        disposed = true;
    }
}