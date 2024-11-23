using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Utils;
using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Utils;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Rendering.Worlds;

public class WorldRenderer(MainRenderer mainRenderer) : IDisposable
{
    public Profiler Profiler => mainRenderer.Profiler;
    
	public int HorizontalDistance = 16;
	public int VerticalDistance = 8;
    
    private readonly MainRenderer mainRenderer = mainRenderer;
    private readonly MixedBindings backgroundBindings = Primitives.InWorld.GenerateSphere(null, 1.0f, 16, 16);   
    private readonly FloatBindings cloudBindings = Primitives.OfVector3.GeneratePlane(null);
    
    private World? World => mainRenderer.Game.World;

    private ChunkRenderer[,,] chunkRenderers = new ChunkRenderer[0, 0, 0];
    private ChunkRenderer.Comparer chunkRendererComparer = new(Vector3i.Zero);
    private List<ChunkRenderer> sortedChunkRenderers = [];
    private List<ChunkRenderer> sortedCulledChunkRenderers = [];
	private int minX, minY, minZ;
	private int maxX, maxY, maxZ;
	private int lastCullX, lastCullY, lastCullZ;
	private bool forceCulling = true;

	private int horizontalRenderRange;
	private int verticalRenderRange;
    
#region Chunk Rendering
    private int WrapHorizontal(int x) => MathUtil.FloorMod(x + HorizontalDistance, horizontalRenderRange);
    
    private int WrapVertical(int y) => MathUtil.FloorMod(y + VerticalDistance, verticalRenderRange);
    
    public void RefreshRenderers()
    {
        Profiler.Push("refresh-renderers");
        foreach (ChunkRenderer chunkRenderer in chunkRenderers)
            chunkRenderer.Dispose();
        sortedChunkRenderers.Clear();

        if (World is null)
        {
            chunkRenderers = new ChunkRenderer[0, 0, 0];
            return;
        }

        minX = minZ = -HorizontalDistance;
        maxX = maxZ = HorizontalDistance;
        minY = -VerticalDistance;
        maxY = VerticalDistance;
        horizontalRenderRange = HorizontalDistance * 2 + 1;
        verticalRenderRange = VerticalDistance * 2 + 1;

        chunkRenderers = new ChunkRenderer[horizontalRenderRange, verticalRenderRange, horizontalRenderRange];
        for (int x = -HorizontalDistance; x <= HorizontalDistance; x++)
        for (int y = -VerticalDistance; y <= VerticalDistance; y++)
        for (int z = -HorizontalDistance; z <= HorizontalDistance; z++)
        {
            ChunkRenderer chunkRenderer = new(World, x, y, z);
            chunkRenderers[x + HorizontalDistance, y + VerticalDistance, z + HorizontalDistance] = chunkRenderer;
            sortedChunkRenderers.Add(chunkRenderer);
        }
        
        UpdateChunkPositions();
        Profiler.Pop();
    }

    public void UpdateChunkPositions()
    {
        Profiler.Push("move-chunks");
        Vector3d cameraPosition = mainRenderer.Camera.Position;
        int x = (int)Math.Floor(cameraPosition.X / Chunk.Length);
        int y = (int)Math.Floor(cameraPosition.Y / Chunk.Length);
        int z = (int)Math.Floor(cameraPosition.Z / Chunk.Length);
        Vector3i cameraChunkPositionInt = new(x, y, z);
        chunkRendererComparer.CameraPosition = cameraChunkPositionInt;
        
        minX = x - HorizontalDistance;
        minY = y - VerticalDistance;
        minZ = z - HorizontalDistance;
        maxX = x + HorizontalDistance;
        maxY = y + VerticalDistance;
        maxZ = z + HorizontalDistance;
        
        for (int ax = 0; ax < horizontalRenderRange; ax++)
        for (int ay = 0; ay < verticalRenderRange; ay++)
        for (int az = 0; az < horizontalRenderRange; az++)
        {
            int cx = ax + x - HorizontalDistance;
            int cy = ay + y - VerticalDistance;
            int cz = az + z - HorizontalDistance;

            ChunkRenderer chunkRenderer = chunkRenderers[
                WrapHorizontal(cx),
                WrapVertical(cy),
                WrapHorizontal(cz)
            ];
            if (chunkRenderer.X == cx
                && chunkRenderer.Y == cy
                && chunkRenderer.Z == cz)
                continue;

            chunkRenderer.SetPosition(cx, cy, cz);
        }

        Profiler.Push("sort");
        sortedChunkRenderers.Sort(chunkRendererComparer);
        Profiler.Pop();
        Profiler.Pop();
    }

    public void Draw(GlState? parentState)
    {
        Game game = mainRenderer.Game;
        
        if (HorizontalDistance != game.Settings.RenderDistanceHorizontal.Value
            || VerticalDistance != game.Settings.RenderDistanceVertical.Value)
        {
            HorizontalDistance = game.Settings.RenderDistanceHorizontal;
            VerticalDistance = game.Settings.RenderDistanceVertical;
            RefreshRenderers();
        }

        sortedCulledChunkRenderers = sortedChunkRenderers;

        int targetFps = 60;
        if (game.Settings.FpsLimit > 0)
            targetFps = game.Settings.FpsLimit;
        targetFps = Math.Max(targetFps, 60);
        
        double targetRenderTime = game.UpdateStartTime;
        targetRenderTime += Math.Max((1.0 / targetFps) - (Game.TimeElapsed - targetRenderTime), 0.0);
        
        Profiler.Push("render");
        foreach (ChunkRenderer chunkRenderer in sortedChunkRenderers)
        {
            if (!chunkRenderer.NeedsUpdate || !chunkRenderer.IsInCamera(mainRenderer.Camera))
                continue;

            chunkRenderer.Render(Profiler);
            game.CurrentChunkUpdates++;
            
            if (Game.TimeElapsed > targetRenderTime)
                break;
        }

        Profiler.PopPush("draw");
        
        Matrix4 viewProjection = mainRenderer.Camera.ViewProjection;
        Vector3d cameraPosition = mainRenderer.Camera.Position;

        var worldShader = mainRenderer.Shaders.Get<WorldShader>();
        worldShader.Use();
        
        double currentTime = Game.TimeElapsed;
        for (int pass = 0; pass < 3; pass++)
        {
            bool translucent = pass > 0;
            using GlState? state = translucent ? new GlState(parentState) : null;
            state?.Enable(EnableCap.Blend);

            switch (pass)
            {
                case 1:
                    state?.ColorMask(false, false, false, false);
                    break;
                
                case 2:
                    state?.DepthMask(false);
                    state?.DepthFunc(DepthFunction.Equal);
                    break;
            }

            foreach (ChunkRenderer chunkRenderer in sortedChunkRenderers)
            {
                if (translucent ? chunkRenderer.EmptyTranslucent : chunkRenderer.EmptyOpaque)
                    continue;

                if (!chunkRenderer.IsInCamera(mainRenderer.Camera))
                    continue;

                Vector3 offset = (Vector3)((Vector3d)chunkRenderer.Position * Chunk.Length - cameraPosition);
                Matrix4 mvp = Matrix4.CreateTranslation(offset) * viewProjection;

                worldShader.Mvp.Set(mvp);
                worldShader.CameraPosition.Set(-offset);
                worldShader.Fade.Set((float)chunkRenderer.GetFadeAmount(currentTime));
                
                if (translucent)
                    chunkRenderer.DrawTranslucent();
                else
                    chunkRenderer.DrawOpaque();
            }
        }
        
        worldShader.Mvp.Set(mainRenderer.Camera.ViewProjection);
        worldShader.CameraPosition.Set(Vector3.Zero);
        worldShader.Fade.Set(0.0f);
        Shader.Stop();
        Profiler.Pop();
    }
    
    public void MarkNeighborUpdateAt(int x, int y, int z)
    {
        MarkUpdateAt(x, y, z);
        MarkUpdateAt(x - 1, y, z);
        MarkUpdateAt(x + 1, y, z);
        MarkUpdateAt(x, y - 1, z);
        MarkUpdateAt(x, y + 1, z);
        MarkUpdateAt(x, y, z - 1);
        MarkUpdateAt(x, y, z + 1);
    }
    
    public void MarkUpdateAt(int x, int y, int z)
    {
        MarkChunkUpdateAt(x >> Chunk.LengthPow2, y >> Chunk.LengthPow2, z >> Chunk.LengthPow2);
    }
    
    public void MarkNeighborChunkUpdateAt(int x, int y, int z)
    {
        MarkChunkUpdateAt(x, y, z);
        MarkChunkUpdateAt(x - 1, y, z);
        MarkChunkUpdateAt(x + 1, y, z);
        MarkChunkUpdateAt(x, y - 1, z);
        MarkChunkUpdateAt(x, y + 1, z);
        MarkChunkUpdateAt(x, y, z - 1);
        MarkChunkUpdateAt(x, y, z + 1);
    }
    
    public void MarkChunkUpdateAt(int x, int y, int z)
    {
        if (x < minX || x > maxX || y < minY || y > maxY || z < minZ || z > maxZ)
            return;
        
        ChunkRenderer chunkRenderer = chunkRenderers[
            WrapHorizontal(x),
            WrapVertical(y),
            WrapHorizontal(z)
        ];
        if (chunkRenderer.X == x && chunkRenderer.Y == y && chunkRenderer.Z == z)
        {
            forceCulling = true;
            chunkRenderer.NeedsUpdate = true;
        }
    }
#endregion

    public void DrawSky(GlState? parentState)
    {
        using GlState state = new(parentState);
        mainRenderer.Shaders.Get<SkyShader>().Use();
        state.Disable(EnableCap.DepthTest);
        state.CullFace(CullFaceMode.Front);
        backgroundBindings.Draw();
    }
    
    public void DrawClouds(GlState? parentState)
    {
        using GlState state = new(parentState);
        mainRenderer.Shaders.Get<CloudShader>().Use();
        state.Enable(EnableCap.Blend);
        state.Disable(EnableCap.CullFace);
        cloudBindings.Draw();
    }
    
    public void Dispose()
    {
        backgroundBindings.Dispose();
        cloudBindings.Dispose();
        foreach (ChunkRenderer chunkRenderer in chunkRenderers)
            chunkRenderer.Dispose();
        sortedChunkRenderers.Clear();

        GC.SuppressFinalize(this);
    }
}