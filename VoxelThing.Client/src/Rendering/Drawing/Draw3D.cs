using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Rendering.Drawing;

public class Draw3D : IDisposable
{
    private readonly MainRenderer renderer;

    public readonly FloatBindings WorldBindings = new(VertexLayout.World);
    private readonly FloatBindings billboardBindings = new(new(VertexType.Vector3));
    private readonly FloatBindings lineBindings = new(new(VertexType.Vector3), PrimitiveType.Lines);

    private readonly BillboardShader billboardShader;
    private readonly WorldShader worldShader;
    private readonly LineShader lineShader;

    public Draw3D(MainRenderer renderer)
    {
        this.renderer = renderer;

        billboardShader = renderer.Shaders.Get<BillboardShader>();
        worldShader = renderer.Shaders.Get<WorldShader>();
        lineShader = renderer.Shaders.Get<LineShader>();

        billboardBindings.Put(
            1.0f, 1.0f, 0.0f,
            1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f
        );
        billboardBindings.AddIndices(0, 1, 2, 2, 3, 0);
        billboardBindings.Upload(false);
    }

    public void AddVertex(float x, float y, float z, float r, float g, float b, float u, float v)
        => WorldBindings.Put(x, y, z, r, g, b, u, v);
    
    public void AddIndex(int i) => WorldBindings.AddIndex(i);

    public void AddIndices(params int[] i) => WorldBindings.AddIndices(i);

    public void Setup()
    {
        billboardShader.Use();
        billboardShader.Projection.Set(renderer.Camera.Projection);
        renderer.SetupFogInfo(in billboardShader.FogInfo);
        
        lineShader.Use();
        lineShader.ViewProjection.Set(renderer.Camera.ViewProjection);
        lineShader.ViewportSize.Set(renderer.Game.ClientSize);
        
        Shader.Stop();
    }

    public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        worldShader.Use();
        WorldBindings.Upload(true);
        WorldBindings.PrimitiveType = primitiveType;
        WorldBindings.Draw();
        WorldBindings.PrimitiveType = PrimitiveType.Triangles;
    }

    public void DrawBillboard(Billboard billboard)
    {
        billboardShader.Use();
        billboard.ApplyToShader(billboardShader, renderer.Camera.View);

        if (billboard.Texture is not null) billboard.Texture.Use();
        else Texture.Stop();

        billboardBindings.Draw();
    }

    public void DrawBoxLines(
        Aabb aabb,
        float r = 0.0f,
        float g = 0.0f,
        float b = 0.0f,
        float a = 1.0f,
        float thickness = 1.0f
    )
    {
        Vector3 min = (Vector3)(aabb.Min - renderer.Camera.Position);
        Vector3 max = (Vector3)(aabb.Max - renderer.Camera.Position);
        
        lineBindings.Put(
            min.X, min.Y, min.Z,
            min.X, min.Y, max.Z,
            min.X, max.Y, min.Z,
            min.X, max.Y, max.Z,
            max.X, min.Y, min.Z,
            max.X, min.Y, max.Z,
            max.X, max.Y, min.Z,
            max.X, max.Y, max.Z
        );
        lineBindings.AddIndices(
            0b000, 0b001, 0b000, 0b100,
            0b101, 0b100, 0b101, 0b001,
            0b111, 0b110, 0b111, 0b011,
            0b010, 0b011, 0b010, 0b110,
            0b000, 0b010, 0b001, 0b011,
            0b100, 0b110, 0b101, 0b111
        );
        lineBindings.Upload(true);
        
        lineShader.Use();
        lineShader.Thickness.Set(thickness);
        lineShader.Color.Set(r, g, b, a);
        lineBindings.Draw();
        Shader.Stop();
    }

    public void Dispose()
    {
        WorldBindings.Dispose();
        billboardBindings.Dispose();
        GC.SuppressFinalize(this);
    }
}