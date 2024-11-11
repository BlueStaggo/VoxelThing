namespace VoxelThing.Game.Maths;

public class NibbleArray3D : Array3D<byte>
{
    public int HalfLength { get; }

    public NibbleArray3D(int width)
        : this(width, width, width) { }

    public NibbleArray3D(int width, int height, int length)
        : this(new byte[width * height * (length + 1) / 2], width, height, length) { }

    public NibbleArray3D(byte[] data, int width, int height, int length) : base(data)
    {
        if (data.Length < width * height * (length + 1) / 2)
            throw new ArgumentException("Array is too small!", nameof(data));
        
        Data = data;
        Width = width;
        Height = height;
        Length = length;
        HalfLength = (length + 1) / 2;
    }

    public override byte this[int x, int y, int z]
    {
        get
        {
            int i = (x * Height + y) * HalfLength + z / 2;
            byte nibblePair = Data[i];
            if ((z & 1) == 0) nibblePair >>= 4;
            return (byte)(nibblePair & 0xF);
        }
        set
        {
            int i = (x * Height + y) * HalfLength + z / 2;
            byte nibblePair = Data[i];
            if ((z & 1) == 0) Data[i] = (byte)(nibblePair & 0xF0 | value & 0xF);
            else Data[i] = (byte)(nibblePair & 0xF | (value & 0xF) << 4);
        }
    }
}