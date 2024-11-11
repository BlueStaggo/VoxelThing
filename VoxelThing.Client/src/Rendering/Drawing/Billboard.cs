using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Textures;

namespace VoxelThing.Client.Rendering.Drawing;

public readonly struct Billboard()
{
    public Vector3 Position { get; init; }
    public Vector2 Size { get; init; } = Vector2.One;
    public Vector2 Align { get; init; } = new(0.5f, 0.5f);
    public Vector4 Color { get; init; } = Vector4.One;
    public Vector4 Uv { get; init; } = new(0.0f, 0.0f, 1.0f, 1.0f);
    public Texture? Texture { get; init; }
    public bool Spherical { get; init; }

    public Vector3 ColorRgb
    {
        get => Color.Xyz;
        init => Color = new(value.X, value.Y, value.Z, 1.0f);
    }

    public void ApplyToShader(BillboardShader shader, Matrix4 view)
    {
        if (Texture is not null) Texture.Use();
        else Texture.Stop();

        Matrix4 modelView = Matrix4.CreateTranslation(Position) * view;
        modelView.M11 = 1.0f;
        modelView.M12 = 0.0f;
        modelView.M13 = 0.0f;
        if (Spherical)
        {
            modelView.M21 = 0.0f;
            modelView.M22 = 1.0f;
            modelView.M23 = 0.0f;
        }
        modelView.M31 = 0.0f;
        modelView.M32 = 0.0f;
        modelView.M33 = 1.0f;

        shader.ModelView.Set(modelView);
		shader.Position.Set(Position);
		shader.Size.Set(Size);
		shader.Align.Set(Align);
		shader.HasTexture.Set(Texture is not null);
		shader.Color.Set(Color);
		shader.UvRange.Set(Uv);
    }
}