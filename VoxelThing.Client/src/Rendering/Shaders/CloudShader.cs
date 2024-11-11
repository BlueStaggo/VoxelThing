using VoxelThing.Client.Rendering.Shaders.Modules;

namespace VoxelThing.Client.Rendering.Shaders;

public class CloudShader : Shader
{
    public readonly Matrix4Uniform ViewProj;
    public readonly Vector3Uniform Offset;
    public readonly Vector4Uniform UvRange;
    public readonly FloatUniform CloudHeight;

    public readonly IntUniform Texture;
    public readonly FogInfo FogInfo;

    public CloudShader() : base("cloud")
    {
        Use();
        ViewProj = new(Handle, "viewProj");
        Offset = new(Handle, "offset");
        (UvRange = new(Handle, "uvRange")).Set(0.0f, 0.0f, 0.25f, 0.25f);
        CloudHeight = new(Handle, "cloudHeight");

        (Texture = new(Handle, "tex")).Set(0);
        FogInfo = new(Handle);
        Stop();
    }
}