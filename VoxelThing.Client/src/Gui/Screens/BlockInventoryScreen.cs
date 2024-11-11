using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Drawing;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Gui.Screens;

public class BlockInventory(Game game) : Screen(game)
{
    private const int Rows = 10;
    private const int Columns = 5;
    private static readonly Vector2i GridDimensions = new(Rows, Columns);

    public override void Draw()
    {
        base.Draw();

        MainRenderer renderer = Game.MainRenderer;
        Draw2D draw2D = renderer.Draw2D;

        using (GlState state = new())
        {
            Texture.Stop();
            state.Enable(EnableCap.Blend);
            draw2D.DrawQuad(new()
            {
                Position = Vector2.Zero,
                Size = renderer.ScreenDimensions.Size,
                Color = (0.0f, 0.0f, 0.0f, 0.8f)
            });

            const string title = "SELECT BLOCK";
            renderer.Fonts.Outlined.Print(
                title, renderer.ScreenDimensions.IntWidth / 2.0f, 20.0f,
                align: 0.5f
            );
        }

        Texture hotbarTexture = renderer.Textures.Get("gui/hotbar.png");
        Texture blocksTexture = renderer.Textures.Get("blocks.png");

        Vector2i slotSize = new(hotbarTexture.Size.X / 2, hotbarTexture.Size.Y);
        Vector2i blockOffset = (slotSize - Vector2i.One * 16) / 2;

        Quad hotbarQuad = new()
        {
            Size = slotSize,
            Texture = hotbarTexture,
            Uv = (0.0f, 0.0f, 0.5f, 1.0f)
        };
        Quad blockQuad = new()
        {
            Size = (16.0f, 16.0f),
            Texture = blocksTexture
        };

        for (int x = 0; x < Rows; x++)
        for (int y = 0; y < Columns; y++)
        {
            Vector2 slotPosition = (renderer.ScreenDimensions.Size - slotSize * GridDimensions) / 2.0f + new Vector2(x, y) * slotSize;
            draw2D.DrawQuad(hotbarQuad with
            {
                Position = slotPosition
            });
        }

        for (int x = 0; x < Rows; x++)
        for (int y = 0; y < Columns; y++)
        {
            int i = x + y * Rows;
            if (i >= Block.RegisteredBlocksOrdered.Count) continue;
            
            Block block = Block.RegisteredBlocksOrdered[i];
            
            Vector2 slotPosition = (renderer.ScreenDimensions.Size - slotSize * GridDimensions) / 2.0f + new Vector2(x, y) * slotSize;
            Vector2i textureCoordinates = block.Texture.Get(Direction.North);

            draw2D.DrawQuad(blockQuad with
            {
                Position = slotPosition + blockOffset,
                Uv = MathUtil.UvWithExtents(
                    blocksTexture.UCoord(textureCoordinates.X * Block.TextureResolution),
                    blocksTexture.VCoord(textureCoordinates.Y * Block.TextureResolution),
                    blocksTexture.UCoord(Block.TextureResolution),
                    blocksTexture.VCoord(Block.TextureResolution)
                )
            });
        }
    }

    protected override void OnKeyPressed(KeyboardKeyEventArgs args)
    {
        base.OnKeyPressed(args);

        if (args.Key == Keys.E)
        {
            Game.CurrentScreen = Parent;
            return;
        }

        int newIndex = Game.HeldItem;

        if (args.Key >= Keys.D1 && args.Key <= Keys.D9)
            newIndex = args.Key - Keys.D1;
        else if (args.Key == Keys.D0)
            newIndex = 9;
        
        if (newIndex >= 0 && newIndex < Game.Palette.Length)
            Game.HeldItem = newIndex;
    }

    protected override void OnMouseClicked(PositionalMouseButtonEventArgs args)
    {
        base.OnMouseClicked(args);

        MainRenderer renderer = Game.MainRenderer;
        Texture hotbarTexture = renderer.Textures.Get("gui/hotbar.png");
        Vector2i slotSize = new(hotbarTexture.Width / 2, hotbarTexture.Height);
        Vector2i blockOffset = (slotSize - Vector2i.One * 16) / 2;

        int mx = args.Position.X;
        int my = args.Position.Y;

        for (int x = 0; x < Rows; x++)
        for (int y = 0; y < Columns; y++)
        {
            Vector2i slotPosition = (renderer.ScreenDimensions.IntSize - slotSize * GridDimensions) / 2 + new Vector2i(x, y) * slotSize;
            if (mx <= slotPosition.X + blockOffset.X
                || mx >= slotPosition.X + slotSize.X - blockOffset.X
                || my <= slotPosition.Y + blockOffset.Y
                || my >= slotPosition.Y + slotSize.Y - blockOffset.Y) continue;
            
            int i = x + y * Rows;
            Block? block = i < Block.RegisteredBlocksOrdered.Count ? Block.RegisteredBlocksOrdered[i] : null;
            Game.Palette[Game.HeldItem] = block;
        }
        
        Game.CurrentScreen = Parent;
    }
}