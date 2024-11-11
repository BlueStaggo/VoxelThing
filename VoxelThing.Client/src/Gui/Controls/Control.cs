using OpenTK.Mathematics;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering.Drawing;

namespace VoxelThing.Client.Gui.Controls;

public class Control(Screen screen)
{
    public Vector2 Position;
    public Vector2 Size;
    public Vector2 AlignPosition;
    public Vector2 AlignSize;

    public Vector2 ScaledPosition
    {
        get
        {
            Vector2 offset = Container?.ScaledPosition ?? Vector2.Zero;
            Vector2 parentSize = Container?.ScaledSize ?? Screen.Game.MainRenderer.ScreenDimensions.IntSize;
            return Position + offset + parentSize * AlignPosition;
        }
    }

    public Vector2 ScaledSize
    {
        get
        {
            Vector2 parentSize = Container?.ScaledSize ?? Screen.Game.MainRenderer.ScreenDimensions.IntSize;
            return Size + parentSize * AlignSize;
        }
    }

    public bool Disabled;
    public event EventHandler<PositionalMouseButtonEventArgs>? OnClick;

    protected readonly Screen Screen = screen;
    internal Container? Container;

    public virtual void Draw() { }

    protected void DrawButtonBackground()
    {
        float color = Disabled ? 0.25f : 0.5f;
        float outlineColor = color / 2.0f;

        Draw2D draw2D = Screen.Game.MainRenderer.Draw2D;

        draw2D.DrawQuad(new()
        {
            Position = ScaledPosition,
            Size = ScaledSize,
            ColorRgb = (outlineColor, outlineColor, outlineColor)
        });
        draw2D.DrawQuad(new()
        {
            Position = ScaledPosition + Vector2.One,
            Size = ScaledSize - Vector2.One * 2.0f,
            ColorRgb = (color, color, color)
        });
    }

    public bool Intersects(Vector2i point) => Intersects(point.X, point.Y);

    public bool Intersects(int x, int y)
    {
        Vector2 position = ScaledPosition;
        Vector2 size = ScaledSize;
        return x >= position.X && x < position.X + size.X
            && y >= position.Y && y < position.Y + size.Y;
    }

    public virtual void CheckMouseClicked(PositionalMouseButtonEventArgs args)
    {
        if (!Disabled && Intersects(args.Position))
            OnClick?.Invoke(this, args);
    }

    public bool IsInContainer(Container container) => this.Container == container;
}
