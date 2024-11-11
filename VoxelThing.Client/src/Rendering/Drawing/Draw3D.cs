using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Rendering.Vertices;

namespace VoxelThing.Client.Rendering.Drawing;

public class Draw3D : IDisposable
{
    private readonly MainRenderer renderer;

    public readonly FloatBindings WorldBindings = new(VertexLayout.WorldFloat);
    private readonly FloatBindings billboardBindings = new(VertexType.Vector3);

    private readonly BillboardShader billboardShader;
    private readonly WorldShader worldShader;

    public Draw3D(MainRenderer renderer)
    {
        this.renderer = renderer;

        billboardShader = renderer.Shaders.Get<BillboardShader>();
        worldShader = renderer.Shaders.Get<WorldShader>();

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
    }

    public void Draw()
    {
        worldShader.Use();
        WorldBindings.Upload(true);
        WorldBindings.Draw();
    }

    public void DrawBillboard(Billboard billboard)
    {
        billboardShader.Use();
        billboard.ApplyToShader(billboardShader, renderer.Camera.View);

        if (billboard.Texture is not null) billboard.Texture.Use();
        else Texture.Stop();

        billboardBindings.Draw();
    }

    public void Dispose()
    {
        WorldBindings.Dispose();
        billboardBindings.Dispose();
        GC.SuppressFinalize(this);
    }
}