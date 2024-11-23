using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Rendering.Vertices;

namespace VoxelThing.Client.Rendering.Drawing;

public class Draw2D : IDisposable
{
    private readonly MainRenderer renderer;

    private readonly FloatBindings screenBindings = new(VertexLayout.Screen);
    private readonly FloatBindings quadBindings = new(new(VertexType.Vector2));

    private readonly QuadShader quadShader;
    private readonly ScreenShader screenShader;

    public bool TextureEnabled { set => screenShader.HasTexture.Set(value); }

    public Draw2D(MainRenderer renderer)
    {
        this.renderer = renderer;

        quadShader = renderer.Shaders.Get<QuadShader>();
        screenShader = renderer.Shaders.Get<ScreenShader>();

        quadBindings.Put(
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f
        );
        quadBindings.AddIndices(0, 1, 2, 2, 3, 0);
        quadBindings.Upload(false);
    }

    public void AddVertex(float x, float y, float r, float g, float b, float u, float v)
        => screenBindings.Put(x).Put(y).Put(r).Put(g).Put(b).Put(u).Put(v);
    
    public void AddIndex(int i) => screenBindings.AddIndex(i);

    public void AddIndices(params int[] i) => screenBindings.AddIndices(i);

    public void AddIndividualQuadIndices() => screenBindings.AddIndividualQuadIndices();

    public void AddQuadIndices() => screenBindings.AddQuadIndices();

    public void Setup()
    {
        quadShader.Use();
        quadShader.ScreenScale.Set(renderer.ScreenDimensions.Scale);
    }

    public void Draw()
    {
        screenShader.Use();
        screenBindings.Upload(true);
        screenBindings.Draw();
    }

    public void DrawQuad(Quad quad)
    {
        quadShader.Use();
        quad.ApplyToShader(quadShader, renderer.ScreenDimensions.ViewProj);

        if (quad.Texture is not null) quad.Texture.Use();
        else Texture.Stop();

        quadBindings.Draw();
    }

    public void Dispose()
    {
        screenBindings.Dispose();
        quadBindings.Dispose();
        GC.SuppressFinalize(this);
    }
}