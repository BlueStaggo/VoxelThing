using StbImageSharp;

namespace VoxelThing.Client;

public static class Extensions
{
    public static uint GetRgba(this ImageResult image, int x, int y)
    {
        int i = (x + y * image.Width) * 4;
        return (uint)image.Data[i + 0] << 24
               | (uint)image.Data[i + 1] << 16
               | (uint)image.Data[i + 2] << 8
               | image.Data[i + 3];
    }
}