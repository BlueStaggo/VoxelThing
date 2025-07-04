using MemoryPack;

namespace VoxelThing.Game.Maths;

[MemoryPackable]
public partial class Array3D<T>
{
    public T[] Data { get; protected init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Length { get; init; }

    protected Array3D(T[] data)
    {
        Data = data;
    }

    public Array3D(int width)
        : this(width, width, width) { }

    public Array3D(int width, int height, int length)
        : this(new T[width * height * length], width, height, length) { }

    [MemoryPackConstructor]
    public Array3D(T[] data, int width, int height, int length)
    {
        if (data.Length < width * height * length)
            throw new ArgumentException("Array is too small!", nameof(data));
        
        Data = data;
        Width = width;
        Height = height;
        Length = length;
    }

    public virtual T this[int x, int y, int z]
    {
        get => Data[(x * Height + y) * Length + z];
        set => Data[(x * Height + y) * Length + z] = value;
    }
}