using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Drawing;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Networking;
using VoxelThing.Game.Worlds;

namespace VoxelThing.Client.Gui.Screens;

public class IngameScreen(Game game) : Screen(game)
{
    private readonly int[] prevHoverProgress = new int[game.Palette.Length];
    private readonly int[] hoverProgress = new int[game.Palette.Length];
    private int swingTick = 0;

    protected override void OnKeyPressed(KeyboardKeyEventArgs args)
    {
        int newIndex = Game.HeldItem;
        Keys key = args.Key;

        newIndex = key switch
        {
            >= Keys.D1 and <= Keys.D9 => key - Keys.D1,
            Keys.D0 => 9,
            _ => newIndex
        };

        if (newIndex >= 0 && newIndex < Game.Palette.Length)
            Game.HeldItem = newIndex;
    }

    public override void Tick()
    {
        for (int i = 0; i < Game.Palette.Length; i++)
        {
            int hover = hoverProgress[i];
            prevHoverProgress[i] = hover;

            if (i == Game.HeldItem)
                hover++;
            else
                hover--;

            hover = int.Clamp(hover, 0, Game.TicksPerSecond / 4);
            hoverProgress[i] = hover;
        }

        if (swingTick > 0)
        {
            swingTick--;
        }
    }

    public override void Draw()
    {
        base.Draw();

        if (!Game.Settings.ThirdPerson)
        {
            DrawCrosshair();
            DrawHand();
        }

        DrawHotbar();
    }

    private void DrawCrosshair()
    {
        MainRenderer renderer = Game.MainRenderer;
        Texture crosshairTexture = renderer.Textures.Get("gui/crosshair.png");
        renderer.Draw2D.DrawQuad(new()
        {
            Position = (
                (renderer.ScreenDimensions.IntWidth - crosshairTexture.Width) / 2.0f,
                (renderer.ScreenDimensions.IntHeight - crosshairTexture.Height) / 2.0f
            ),
            Texture = crosshairTexture
        });
    }

    private void DrawHand()
    {
        if (Game.Player is null)
            return;
        
        MainRenderer renderer = Game.MainRenderer;

        Block? block = GetPlacedBlock();
        Texture playerTexture = renderer.Textures.Get($"entities/{Game.Player.Texture}.png");
        Texture blocksTexture = renderer.Textures.Get("blocks.png");

        float handSize = renderer.ScreenDimensions.IntHeight;
        float hover =
            float.Lerp(prevHoverProgress[Game.HeldItem], hoverProgress[Game.HeldItem],
                (float)Game.PartialTick) / (Game.TicksPerSecond / 4.0f);
        hover = MathUtil.SquareOut(hover);
        double swing = Math.Max(swingTick - Game.PartialTick, 0.0) / 10.0;
        swing *= swing;

        float bobX = (float)(Math.Sin(swing * Math.PI) / -2.0) * handSize;
        float bobY = (float)((1.0 - hover * (block == null ? 1.0 : 0.9))
                             + (Math.Sin(swing * 2.0 * Math.PI) / 9.0)) * handSize;

        if (Game.Settings.ViewBobbing)
        {
            double renderWalk = Game.Player.RenderWalk.GetInterpolatedValue(Game.PartialTick);
            double fallAmount = Game.Player.FallAmount.GetInterpolatedValue(Game.PartialTick);
            bobX += (float)(renderWalk * -0.02) * handSize;
            bobY += (float)((Math.Abs(renderWalk) * 0.025)
                            + Math.Max(fallAmount * 0.1, -0.25)) * handSize;
            bobX -= (float)Game.Player.LookYawOffset.GetInterpolatedValue(Game.PartialTick) * handSize * 0.0025f;
            bobY += (float)Game.Player.LookPitchOffset.GetInterpolatedValue(Game.PartialTick) * handSize * 0.0025f;
        }

        renderer.Draw2D.DrawQuad(new()
        {
            Position = (
                renderer.ScreenDimensions.IntWidth - handSize + bobX,
                renderer.ScreenDimensions.IntHeight - handSize + bobY + handSize / 4.0f
            ),
            Size = (handSize, handSize),
            Texture = playerTexture,
            Uv = (1.0f - playerTexture.UCoord(32), 1.0f - playerTexture.VCoord(32), 1.0f, 1.0f)
        });

        if (block is null) return;
        Vector2i texture = block.Texture.Get(Direction.North);

        float minU = blocksTexture.UCoord(texture.X * 16);
        float minV = blocksTexture.VCoord(texture.Y * 16);
        float maxU = minU + blocksTexture.UCoord(16);
        float maxV = minV + blocksTexture.VCoord(16);

        Quad blockQuad = new()
        {
            Position = (
                renderer.ScreenDimensions.Width + (bobX - handSize / 1.8f),
                renderer.ScreenDimensions.Height + (bobY - handSize / 1.8f)
            ),
            Size = (handSize * 0.35f, handSize * 0.4f),
            Texture = blocksTexture,
            Uv = (minU, minV, maxU, maxV)
        };

        renderer.Draw2D.DrawQuad(blockQuad);
        renderer.Draw2D.DrawQuad(blockQuad with
        {
            Position = blockQuad.Position + new Vector2(handSize * 0.35f, 0.0f),
            Size = (handSize * 0.15f, handSize * 0.4f),
            ColorRgb = (0.75f, 0.75f, 0.75f)
        });
    }

    private void DrawHotbar()
    {
        MainRenderer renderer = Game.MainRenderer;

        Texture hotbarTexture = renderer.Textures.Get("gui/hotbar.png");
        Texture blocksTexture = renderer.Textures.Get("blocks.png");
        int slotWidth = hotbarTexture.Width / 2;
        int slotHeight = hotbarTexture.Height;

        float startX = (renderer.ScreenDimensions.Width - slotWidth * Game.Palette.Length) / 2.0f;
        float startY = renderer.ScreenDimensions.Height - slotHeight - 5;

        Quad hotbarQuad = new()
        {
            Position = (startX, startY),
            Size = (slotWidth, slotHeight),
            Texture = hotbarTexture,
            Uv = (0.0f, 0.0f, 0.5f, 1.0f)
        };

        Quad blockQuad = new()
        {
            Position = (startX + (slotWidth - 16) / 2.0f, startY + (slotHeight - 16) / 2.0f),
            Size = (16, 16),
            Texture = blocksTexture
        };

        for (int i = 0; i < Game.Palette.Length; i++)
        {
            Block? block = Game.Palette[i];

            float hover = float.Lerp(prevHoverProgress[i], hoverProgress[i], (float)Game.PartialTick) /
                          (Game.TicksPerSecond / 4.0f);
            hover = MathUtil.SquareOut(hover);
            hover *= slotHeight / 4.0f;
            Vector2 offset = new(i * slotWidth, -hover);

            float slotOffset = i == Game.HeldItem ? 0.5f : 0.0f;
            renderer.Draw2D.DrawQuad(hotbarQuad with
            {
                Position = hotbarQuad.Position + offset,
                Uv = (slotOffset, 0.0f, 0.5f + slotOffset, 1.0f)
            });

            if (block is null) continue;
            Vector2i texture = block.Texture.Get(Direction.North);

            float minU = blocksTexture.UCoord(texture.X * 16);
            float minV = blocksTexture.VCoord(texture.Y * 16);
            float maxU = minU + blocksTexture.UCoord(16);
            float maxV = minV + blocksTexture.VCoord(16);

            renderer.Draw2D.DrawQuad(blockQuad with
            {
                Position = blockQuad.Position + offset,
                Uv = (minU, minV, maxU, maxV)
            });
        }
    }

    private Block? GetPlacedBlock()
    {
        if (Game.HeldItem < 0 || Game.HeldItem >= Game.Palette.Length)
            return null;

        return Game.Palette[Game.HeldItem];
    }

    protected override void OnMouseClicked(PositionalMouseButtonEventArgs args)
    {
        if (args.Mouse.Button == MouseButton.Button1)
            swingTick = 10;

        if (Game.World is null || Game.Player is null)
            return;

        BlockRaycastResult raycast = Game.SelectionCast;
        if (!raycast.Hit) return;
        
        int x = raycast.HitX;
        int y = raycast.HitY;
        int z = raycast.HitZ;
        Direction face = raycast.HitFace;

        switch (args.Button)
        {
            case MouseButton.Button1:
            {
                Game.World.SetBlock(x, y, z, null);
                Game.PacketHandler?.Server.SendPacket(new CSetBlock(x, y, z, Block.AirId));
                break;
            }
            case MouseButton.Button2:
            {
                Block? placedBlock = GetPlacedBlock();
                if (placedBlock is null) return;
            
                x += face.GetX();
                y += face.GetY();
                z += face.GetZ();

                if (Game.World.GetBlock(x, y, z) is not null) return;
                Game.World.SetBlock(x, y, z, placedBlock);
                Game.PacketHandler?.Server.SendPacket(new CSetBlock(x, y, z, placedBlock.Id));
                swingTick = 10;
                break;
            }
        }
    }
}