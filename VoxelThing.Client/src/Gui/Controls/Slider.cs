using OpenTK.Mathematics;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering.Drawing;

namespace VoxelThing.Client.Gui.Controls;

public class Slider : FocusableControl
{
    public string Text = string.Empty;
    public float Value;
    
    public Font Font;
    public float KnobWidth = 6.0f;
    public float Thickness = 4.0f;

    public override bool DragFocusOnly => true;

    public Slider(Screen screen) : base(screen)
    {
        Font = screen.Game.MainRenderer.Fonts.Outlined;

        OnClick += (_, args) => InvokeMouseDragged(new(args.Mouse.Button, args.Position));
        OnMouseDragged += (_, args) =>
        {
            float scaledX = ScaledPosition.X + KnobWidth / 2.0f;
            float scaledWidth = ScaledSize.X - KnobWidth;
            Value = float.Clamp((args.Position.X - scaledX) / scaledWidth, 0.0f, 1.0f);
        };
    }

    public override void Draw()
    {
        Draw2D draw2D = Screen.Game.MainRenderer.Draw2D;
        Vector2 position = ScaledPosition;
        Vector2 size = ScaledSize;
        float barX = position.X + Value * (size.X - KnobWidth);

        draw2D.DrawQuad(new()
        {
            Position = (position.X, position.Y + (size.Y - Thickness) / 2.0f),
            Size = (size.X, Thickness),
            ColorRgb = (0.0f, 0.0f, 0.0f)
        });
        draw2D.DrawQuad(new()
        {
            Position = (barX, position.Y),
            Size = (KnobWidth, size.Y),
            ColorRgb = (0.0f, 0.0f, 0.0f)
        });
        draw2D.DrawQuad(new()
        {
            Position = (barX + 1.0f, position.Y + 1.0f),
            Size = (KnobWidth - 2.0f, size.Y - 2.0f),
            ColorRgb = (1.0f, 1.0f, 1.0f)
        });

        Font.Print(
            Text,
            position.X + (size.X - Font.GetStringLength(Text)) / 2.0f,
            position.Y + (size.Y - Font.LineHeight) / 2.0f
        );
    }
}