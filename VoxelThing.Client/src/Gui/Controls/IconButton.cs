using OpenTK.Mathematics;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Gui.Controls;

public class IconButton(Screen screen) : Control(screen)
{
    public Vector2i Icon;

    public override void Draw()
    {
        DrawButtonBackground();

        MainRenderer renderer = Screen.Game.MainRenderer;
        Texture iconTexture = renderer.Textures.Get("gui/icons.png");

        renderer.Draw2D.DrawQuad(new()
        {
            Position = ScaledPosition + (ScaledSize - Vector2.One * 16.0f) / 2.0f,
            Size = (16, 16),
            Texture = iconTexture,
            Uv = MathUtil.UvWithExtents(
                iconTexture.UCoord(Icon.X * 16),
                iconTexture.VCoord(Icon.Y * 16),
                iconTexture.UCoord(16),
                iconTexture.VCoord(16)
            )
        });
    }
}