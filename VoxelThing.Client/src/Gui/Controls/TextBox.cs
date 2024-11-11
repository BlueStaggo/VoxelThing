using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering.Drawing;

namespace VoxelThing.Client.Gui.Controls;

public class TextBox : FocusableControl
{
    public string Text = string.Empty;
    public Font Font;

    public TextBox(Screen screen) : base(screen)
    {
        Font = screen.Game.MainRenderer.Fonts.Shadowed;

        OnKeyPressed += (_, args) =>
        {
            if (args.Key == Keys.Backspace && Text.Length > 0)
                Text = Text[..^1];
        };
        OnCharacterTyped += (_, args) => Text += (char)args.Unicode;
    }

    public override void Draw()
    {
        Draw2D draw2D = Screen.Game.MainRenderer.Draw2D;
        Vector2 position = ScaledPosition;
        Vector2 size = ScaledSize;

        float color = 0.25f;
        string printText = Text;

        if (Focused)
        {
            color = 1.0f;
            if (Game.TimeElapsed % 0.4 < 0.2)
                printText += "|";
        }

        draw2D.DrawQuad(new()
        {
            Position = position,
            Size = size,
            ColorRgb = (color, color, color)
        });
        draw2D.DrawQuad(new()
        {
            Position = position + Vector2.One,
            Size = size - Vector2.One * 2.0f,
            ColorRgb = (0.0f, 0.0f, 0.0f)
        });

        Font.Print(
            printText,
            position.X + 5.0f,
            position.Y + (size.Y - Font.LineHeight) / 2.0f
        );
    }
}