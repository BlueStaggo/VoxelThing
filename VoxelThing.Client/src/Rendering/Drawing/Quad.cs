using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Textures;

namespace VoxelThing.Client.Rendering.Drawing;

public readonly struct Quad()
{
    public Vector2 Position { get; init; }
    public Vector2 Size { get; init; }
    public Vector4 Color { get; init; } = Vector4.One;
    public Vector4 Uv { get; init; } = new(0.0f, 0.0f, 1.0f, 1.0f);
    public Texture? Texture { get; init; }

    public Vector3 ColorRgb
    {
        get => Color.Xyz;
        init => Color = new(value.X, value.Y, value.Z, 1.0f);
    }

    public void ApplyToShader(QuadShader shader, Matrix4 viewProj)
    {
        Vector2 size = Size;
        if (Texture is not null)
        {
            Texture.Use();
            if (Size == Vector2.Zero)
                size = Texture.Size;
        }
        else Texture.Stop();

        shader.ViewProj.Set(viewProj);
		shader.Offset.Set(Position);
		shader.Size.Set(size);
		shader.HasTexture.Set(Texture is not null);
        shader.Color.Set(Color);
		shader.UvRange.Set(Uv);
    }
}