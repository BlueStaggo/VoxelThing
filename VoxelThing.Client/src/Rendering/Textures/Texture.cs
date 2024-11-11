using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Rendering.Textures;

public class Texture : IDisposable
{
    private static int textureInUse;

    public readonly int Width, Height;
    private readonly int handle;
    private bool disposed;

    public Vector2i Size => new(Width, Height);

    public Texture(byte[] data, int width, int height, TextureFlags flags)
    {
        Width = width;
        Height = height;

        handle = GL.GenTexture();
        Use();

        bool hasMipmaps = (flags & TextureFlags.Mipmapped) != 0;
        bool hasAlpha = (flags & TextureFlags.NoAlpha) == 0;
        bool isBilinear = (flags & TextureFlags.Bilinear) != 0;

        GL.TexImage2D(
            TextureTarget.Texture2D, 0,
            hasAlpha ? PixelInternalFormat.Rgba : PixelInternalFormat.Rgb,
            width, height, 0,
            hasAlpha ? PixelFormat.Rgba : PixelFormat.Rgb,
            PixelType.UnsignedByte, data
        );
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)(isBilinear ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)(isBilinear ? TextureMagFilter.Linear : TextureMagFilter.Nearest));

        if (hasMipmaps)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)(isBilinear ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.NearestMipmapLinear));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 4);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        Stop();
    }

    ~Texture() => Dispose();

    public void Use()
    {
        if (disposed || textureInUse == handle) return;
        
        GL.BindTexture(TextureTarget.Texture2D, handle);
        textureInUse = handle;
    }

    public static void Stop()
    {
        if (textureInUse == 0) return;
        
        GL.BindTexture(TextureTarget.Texture2D, 0);
        textureInUse = 0;
    }

    public float UCoord(int x) => x / (float)Width;

    public float VCoord(int y) => y / (float)Height;

    public Vector4 UvCoords(int minX, int minY, int maxX, int maxY)
        => new(UCoord(minX), VCoord(minY), UCoord(maxX), VCoord(maxY));
    
    public Vector4 UvCoordsWithExtents(int x, int y, int width, int height)
        => MathUtil.UvWithExtents(UCoord(x), VCoord(y), UCoord(width), VCoord(height));

    public void Dispose()
    {
        if (!disposed)
        {
            GL.DeleteTexture(handle);
            disposed = true;
        }

        if (textureInUse == handle) Stop();
        GC.SuppressFinalize(this);
    }
}