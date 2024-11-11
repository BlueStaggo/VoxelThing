using VoxelThing.Client.Rendering.Shaders.Modules;

namespace VoxelThing.Client.Rendering.Shaders;

public class BillboardShader : Shader
{
    public readonly Matrix4Uniform ModelView;
    public readonly Matrix4Uniform Projection;
    public readonly Vector3Uniform Position;
    public readonly Vector2Uniform Size;
    public readonly Vector2Uniform Align;
    public readonly Vector4Uniform UvRange;

    public readonly IntUniform Texture;
    public readonly BoolUniform HasTexture;
    public readonly Vector4Uniform Color;
    public readonly FogInfo FogInfo;

    public BillboardShader() : base("billboard")
    {
        Use();
        ModelView = new(Handle, "modelView");
        Projection = new(Handle, "projection");
        Position = new(Handle, "position");
        Size = new(Handle, "size");
        Align = new(Handle, "align");
        UvRange = new(Handle, "uvRange");

        (Texture = new(Handle, "tex")).Set(0);
        HasTexture = new(Handle, "hasTex");
        Color = new(Handle, "color");
        FogInfo = new(Handle);
        Stop();
    }
}