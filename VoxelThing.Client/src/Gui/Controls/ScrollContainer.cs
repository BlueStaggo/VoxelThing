using OpenTK.Mathematics;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Client.Rendering;

namespace VoxelThing.Client.Gui.Controls;

public class ScrollContainer(Screen screen) : Container(screen)
{
    public bool CanScroll => contentHeight > ScaledSize.Y;

    private float contentHeight;
    private float scrollAmount;

    public override Control AddControl(Control control)
    {
        base.AddControl(control);
        control.Position.Y = contentHeight;
        control.AlignPosition.Y = 0.0f;
        control.AlignSize.Y = 0.0f;
        contentHeight += control.Size.Y;
        return control;
    }

    public override void ClearControls()
    {
        base.ClearControls();
        contentHeight = 0.0f;
    }

    public void AddPadding(float height)
        => AddControl(new Control(Screen)
        {
            Size = (0, height)
        });
    
    public void Scroll(float amount)
    {
        if (CanScroll)
            scrollAmount += amount;
        
        float max = Math.Max(contentHeight - ScaledSize.Y, 0.0f);
        scrollAmount = Math.Clamp(scrollAmount, 0.0f, max);
    }

    public override void Draw()
    {
        Scroll(0.0f);

        MainRenderer renderer = Screen.Game.MainRenderer;
        Vector2 position = ScaledPosition;
        Vector2 size = ScaledSize;

        using (GlState state = new())
        {
            state.Scissor(
                (int)((renderer.ScreenDimensions.IntWidth - position.X - size.X) * renderer.ScreenDimensions.Scale),
                (int)((renderer.ScreenDimensions.IntHeight - position.Y - size.Y) * renderer.ScreenDimensions.Scale),
                (int)((int)size.X * renderer.ScreenDimensions.Scale),
                (int)((int)size.Y * renderer.ScreenDimensions.Scale)
            );

            foreach (Control control in Controls)
            {
                control.AlignPosition.Y = 0.0f;
                control.AlignSize.Y = 0.0f;

                control.Position.Y -= scrollAmount;
                if (control.Position.Y + control.Size.Y >= 0 && control.Position.Y < size.Y)
                    control.Draw();
                control.Position.Y += scrollAmount;
            }
        }

        renderer.Draw2D.DrawQuad(new()
        {
            Position = (position.X + size.X - 4.0f, position.Y),
            Size = (4.0f, size.Y),
            ColorRgb = (0.25f, 0.25f, 0.25f)
        });

        if (CanScroll)
        {
            float range = 1.0f / contentHeight * size.Y;
            renderer.Draw2D.DrawQuad(new()
            {
                Position = (position.X + size.X - 4.0f, position.Y + scrollAmount * range),
                Size = (4.0f, size.Y * range),
                ColorRgb = (1.0f, 1.0f, 1.0f)
            });
        }
    }

    public override void CheckMouseClicked(PositionalMouseButtonEventArgs args)
    {
        if (!Intersects(args.Position)) return;
        base.CheckMouseClicked(args with
        {
            Position = args.Position + Vector2i.UnitY * (int)scrollAmount
        });
    }
}