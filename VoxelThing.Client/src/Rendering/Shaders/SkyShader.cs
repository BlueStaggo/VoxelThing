namespace VoxelThing.Client.Rendering.Shaders;

public class SkyShader : Shader
{
    public readonly Matrix4Uniform View;
    public readonly Matrix4Uniform Projection;

    public readonly Vector4Uniform FogColor;
    public readonly Vector4Uniform SkyColor;

    public SkyShader() : base("sky")
    {
        Use();
        View = new(Handle, "view");
        Projection = new(Handle, "projection");

        FogColor = new(Handle, "fogCol");
        SkyColor = new(Handle, "skyCol");
        Stop();
    }
}