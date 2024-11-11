using VoxelThing.Client.Rendering.Shaders.Modules;

namespace VoxelThing.Client.Rendering.Shaders;

public class WorldShader : Shader
{
    public readonly Matrix4Uniform Mvp;

    public readonly IntUniform Texture;
    public readonly BoolUniform HasTexture;
    public readonly FloatUniform Fade;
    public readonly Vector3Uniform CameraPosition;
    public readonly FogInfo FogInfo;

    public WorldShader() : base("world")
    {
        Use();
        Mvp = new(Handle, "mvp");

        (Texture = new(Handle, "tex")).Set(0);
        HasTexture = new(Handle, "hasTex");
        Fade = new(Handle, "fade");
        CameraPosition = new(Handle, "camPos");
        FogInfo = new(Handle);
        Stop();
    }
}