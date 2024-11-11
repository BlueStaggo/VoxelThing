using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace VoxelThing.Client.Rendering.Textures;

public class Framebuffer : IDisposable
{
    private static int framebufferInUse;
    
    public int Texture => textureHandle;

    private readonly int handle;
    private readonly int textureHandle;
    private int width;
    private int height;
    private bool disposed;

    public Framebuffer(int width, int height)
    {
        handle = GL.GenFramebuffer();
        textureHandle = GL.GenTexture();
        Resize(width, height);

        Use();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureHandle, 0);
        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            Console.Error.WriteLine($"Incomplete framebuffer {handle}: {status}");
        Stop();
    }

    ~Framebuffer() => Dispose();

    public void Resize(Vector2i size) => Resize(size.X, size.Y);
    
    public void Resize(int width, int height)
    {
        if (width == this.width || height == this.height) return;
        this.width = width;
        this.height = height;

        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, 0);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Use()
    {
        if (disposed || framebufferInUse == handle) return;
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);    
        framebufferInUse = handle;
    }

    public static void Stop()
    {
        if (framebufferInUse == 0) return;
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        framebufferInUse = 0;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            GL.DeleteFramebuffer(handle);
            GL.DeleteTexture(textureHandle);
            disposed = true;
        }

        if (framebufferInUse == handle) Stop();
        GC.SuppressFinalize(this);
    }
}