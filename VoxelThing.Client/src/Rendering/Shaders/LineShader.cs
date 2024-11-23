namespace VoxelThing.Client.Rendering.Shaders;

public class LineShader : Shader
{
    public readonly Matrix4Uniform ViewProjection;
    public readonly FloatUniform Thickness;
    public readonly Vector2Uniform ViewportSize;
    public readonly Vector4Uniform Color;

    public LineShader() : base("line",
        ShaderTypes.Fragment
        | ShaderTypes.Vertex
        | ShaderTypes.Geometry)
    {
        Use();
        ViewProjection = new(Handle, "mvp");
        Thickness = new(Handle, "thickness");
        ViewportSize = new(Handle, "viewportSize");
        Color = new(Handle, "color");
    }
}