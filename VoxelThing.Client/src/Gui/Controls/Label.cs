using OpenTK.Mathematics;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Gui.Controls;

public class Label(Screen screen) : Control(screen)
{
    public string Text = string.Empty;
    public Font Font = screen.Game.MainRenderer.Fonts.Shadowed;
    public Vector2 AlignText = new(0.5f, 0.5f);
    public bool HasBackground;

    public override void Draw()
    {
        if (HasBackground)
            DrawButtonBackground();

        Vector2 position = ScaledPosition;
        Vector2 size = ScaledSize;

        string[] lines = Text.Split("\n");
        foreach (string line in lines)
        {
            Font.Print(
                line,
                position.X + (size.X - Font.GetStringLength(line)) * AlignText.X,
                position.Y + (size.Y - Font.LineHeight * lines.Length) * AlignText.Y
            );
            position.Y += Font.LineHeight;
        }
    }
}