namespace VoxelThing.Client.Rendering.Shaders;

public class QuadShader : Shader
{
    public readonly Matrix4Uniform ViewProj;
    public readonly Vector2Uniform Offset;
    public readonly Vector2Uniform Size;
    public readonly Vector4Uniform UvRange;
    public readonly FloatUniform ScreenScale;

    public readonly IntUniform Texture;
    public readonly BoolUniform HasTexture;
    public readonly Vector4Uniform Color;

    public QuadShader() : base("quad")
    {
        Use();
        ViewProj = new(Handle, "viewProj");
        Offset = new(Handle, "offset");
        Size = new(Handle, "size");
        UvRange = new(Handle, "uvRange");
        ScreenScale = new(Handle, "screenScale");

        (Texture = new(Handle, "tex")).Set(0);
        HasTexture = new(Handle, "hasTex");
        Color = new(Handle, "color");
        Stop();
    }
}