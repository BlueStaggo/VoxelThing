using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VoxelThing.Client.Assets;
using VoxelThing.Client.Gui;
using VoxelThing.Client.Rendering.Drawing;
using VoxelThing.Client.Rendering.Shaders;
using VoxelThing.Client.Rendering.Shaders.Modules;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Client.Rendering.Worlds;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Utils;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Rendering;

public class MainRenderer : IDisposable
{
    public Profiler Profiler => Game.Profiler;

    public readonly Game Game;
    public readonly Camera Camera;
    public readonly ScreenDimensions ScreenDimensions;

    public readonly TextureManager Textures;
    public readonly ShaderManager Shaders;
    public readonly FontManager Fonts;

    public readonly WorldRenderer WorldRenderer;
    public readonly EntityRenderer EntityRenderer;

    public readonly Draw2D Draw2D;
    public readonly Draw3D Draw3D;

    private Vector4 horizonColor = new(0.7f, 0.9f, 1.0f, 1.0f);
    private Vector4 skyColor = new(0.1f, 0.4f, 1.0f, 1.0f);
    private Vector4 fogColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    private readonly Framebuffer skyFramebuffer;

    private Vector3d previousUpdateLocation;

    public MainRenderer(Game game)
    {
		Game = game;

        Camera = new Camera(game);
        ScreenDimensions = new ScreenDimensions(game);

        Textures = new();
        Shaders = new();
        Fonts = new(this);

        WorldRenderer = new(this);
        EntityRenderer = new(this);

        Draw2D = new(this);
        Draw3D = new(this);

        skyFramebuffer = new(game.ClientSize.X, game.ClientSize.Y);
    }

    public void Draw()
    {
        skyFramebuffer.Resize(Game.ClientSize);

        if (previousUpdateLocation.DistanceToSquared(Camera.Position) > 64.0)
        {
            WorldRenderer.UpdateChunkPositions();
            previousUpdateLocation = Camera.Position;
        }

        Camera.Far = MathF.Sqrt(
            WorldRenderer.HorizontalDistance * WorldRenderer.HorizontalDistance
            + WorldRenderer.VerticalDistance * WorldRenderer.VerticalDistance
        ) * 32.0f;
        
        GL.ClearColor((Color4)skyColor);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (Game.World is not null)
        {
            Profiler.Push("world");
            Render3D();
            Profiler.Pop();
        }
        Render2D();
    }

    private void Render3D()
    {
        if (Game.World is null)
            return;
        
        Draw3D.Setup();

        Profiler.Push("setup");
        SetupSkyShader();
        SetupCloudShader();
        SetupWorldShader();
        Shader.Stop();
        
        using GlState state = new();
        
        state.Enable(EnableCap.CullFace);
        state.Enable(EnableCap.DepthTest);
        state.CullFace(CullFaceMode.Front);
        
        Profiler.PopPush("sky");
        skyFramebuffer.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        WorldRenderer.DrawSky(state);
        Framebuffer.Stop();
        WorldRenderer.DrawSky(state);
        
        Profiler.PopPush("chunks");
        state.CullFace(CullFaceMode.Back);
        Textures.Get("blocks.png", TextureFlags.Mipmapped).Use();
        UseSkyTexture(1);
        WorldRenderer.Draw(state);
        
        if (!Game.Settings.HideHud && Game.SelectionCast.Hit)
        {
            BlockRaycastResult cast = Game.SelectionCast;
            Block? block = Game.World.GetBlock(cast.HitX, cast.HitY, cast.HitZ);
            if (block is not null)
            {
                using GlState boxState = new(state);
                boxState.DepthMask(false);
                boxState.DepthFunc(DepthFunction.Lequal);
                boxState.Enable(EnableCap.Blend);
                boxState.Disable(EnableCap.CullFace);
                Draw3D.DrawBoxLines(
                    block.GetCollisionBox(Game.World, cast.HitX, cast.HitY, cast.HitZ),
                    a: 0.4f, thickness: 4.0f                    
                );
            }
        }
        
        state.Disable(EnableCap.CullFace);

        if (Game.Player is not null)
        {
            Profiler.PopPush("entities");
            string skin = $"entities/{Game.Skins[Game.Settings.Skin]}.png";
            Game.Player.SetTexture(skin);
            if (Game.Settings.ThirdPerson)
                EntityRenderer.RenderEntity(Game.Player);
        }

        Profiler.PopPush("clouds");
        Textures.Get("environment/clouds.png").Use();
        WorldRenderer.DrawClouds(state);
        Shader.Stop();
        
        Profiler.Pop();
    }

    private void Render2D()
    {
        Draw2D.Setup();

        var screenShader = Shaders.Get<ScreenShader>();
        screenShader.Use();
        screenShader.Mvp.Set(ScreenDimensions.ViewProj);
    }
    
#region Shader Setup
    public void SetupFogInfo(in FogInfo fogInfo)
    {
        fogInfo.SkyWidth.Set(Game.ClientSize.X);
        fogInfo.SkyHeight.Set(Game.ClientSize.Y);
        fogInfo.HorizontalDistance.Set(WorldRenderer.HorizontalDistance * Chunk.Length);
        fogInfo.VerticalDistance.Set(WorldRenderer.VerticalDistance * Chunk.Length);
    }

    private void SetupWorldShader()
    {
        var worldShader = Shaders.Get<WorldShader>();
        worldShader.Use();
        worldShader.Mvp.Set(Camera.ViewProjection);
        SetupFogInfo(in worldShader.FogInfo);
    }

    private void SetupSkyShader()
    {
        horizonColor.X = Game.Settings.HorizonR;
        horizonColor.Y = Game.Settings.HorizonG;
        horizonColor.Z = Game.Settings.HorizonB;
        skyColor.X = Game.Settings.SkyR;
        skyColor.Y = Game.Settings.SkyG;
        skyColor.Z = Game.Settings.SkyB;
        fogColor.X = Game.Settings.FogR;
        fogColor.Y = Game.Settings.FogG;
        fogColor.Z = Game.Settings.FogB;
        
        float fogFactor = Game.Settings.RenderDistanceHorizontal * Game.Settings.RenderDistanceHorizontal
                               + Game.Settings.RenderDistanceVertical * Game.Settings.RenderDistanceVertical;
        fogFactor = Math.Clamp(1.0f - fogFactor / 64.0f, 0.0f, 1.0f);
        
        var skyShader = Shaders.Get<SkyShader>();
        skyShader.Use();
        skyShader.View.Set(Camera.View);
        skyShader.Projection.Set(Camera.Projection);
        skyShader.FogColor.Set(Vector4.Lerp(horizonColor, fogColor, fogFactor));
        skyShader.SkyColor.Set(Vector4.Lerp(skyColor, fogColor, fogFactor));
    }

    private void SetupCloudShader()
    {
        Vector3 offset = new(
            (float)MathUtil.FloorMod(Camera.Position.X + Game.TimeElapsed, 4096.0),
            (float)Camera.Position.Y,
            (float)MathUtil.FloorMod(Camera.Position.Z, 4096.0)
        );

        var cloudShader = Shaders.Get<CloudShader>();
        cloudShader.Use();
        cloudShader.ViewProj.Set(Camera.ViewProjection);
        cloudShader.Offset.Set(offset);
        SetupFogInfo(in cloudShader.FogInfo);
    }
#endregion

    public void UseSkyTexture(int i)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + i);
        GL.BindTexture(TextureTarget.Texture2D, skyFramebuffer.Texture);
        GL.ActiveTexture(TextureUnit.Texture0);
    }
    
    public void Dispose()
    {
        Textures.Dispose();
        Shaders.Dispose();

        WorldRenderer.Dispose();

        Draw2D.Dispose();
        Draw3D.Dispose();
        
        skyFramebuffer.Dispose();

        GC.SuppressFinalize(this);
    }
}