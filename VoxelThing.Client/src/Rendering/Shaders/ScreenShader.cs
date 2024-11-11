namespace VoxelThing.Client.Rendering.Shaders;

public class ScreenShader : Shader
{
    public readonly Matrix4Uniform Mvp;

    public readonly IntUniform Texture;
    public readonly BoolUniform HasTexture;

    public ScreenShader() : base("screen")
    {
        Use();
        Mvp = new(Handle, "mvp");

        (Texture = new(Handle, "tex")).Set(0);
        HasTexture = new(Handle, "hasTex");
        Stop();
    }
}