using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Gui.Controls;

namespace VoxelThing.Client.Gui.Screens;

public abstract class Screen(Game game)
{
    public readonly Game Game = game;
    public readonly Screen? Parent = game.CurrentScreen;
    public FocusableControl? FocusedControl { get; internal set; }
    public virtual bool PausesWorld => false;

    protected readonly List<Control> Controls = [];

    public virtual void Draw()
    {
        foreach (Control control in Controls)
            control.Draw();
    }

    public virtual void Tick() { }

    public void HandleInput()
    {
        Vector2i scaledMousePosition = Game.ScaledMousePosition;

        foreach (KeyboardKeyEventArgs args in Game.KeysJustPressed)
        {
            OnKeyPressed(args);
            FocusedControl?.InvokeKeyPressed(args);
        }

        foreach (TextInputEventArgs args in Game.CharactersJustTyped)
        {
            OnCharacterTyped(args);
            FocusedControl?.InvokeCharacterTyped(args);
        }

        foreach (MouseButtonEventArgs args in Game.MouseButtonsJustPressed)
            OnMouseClicked(new(args, scaledMousePosition));
        
        for (MouseButton button = 0; (int)button < 8; button++)
            if (Game.IsMouseButtonDown(button))
                OnMouseDragged(new(button, scaledMousePosition));
        
        if (!Game.IsAnyMouseButtonDown && FocusedControl is not null && FocusedControl.DragFocusOnly)
        {
            FocusableControl oldFocused = FocusedControl;
            FocusedControl = null;
            oldFocused?.InvokeFocusLost();
        }

        if (Game.MouseScroll.Y != 0.0f)
            OnMouseScrolled(Game.MouseScroll.Y);
    }

    public virtual void OnClosed() { }

    protected T AddControl<T>(T control)
        where T : Control
    {
        Controls.Add(control);
        return control;
    }
    
    protected virtual void OnKeyPressed(KeyboardKeyEventArgs args)
    {
        if (args.Key == Keys.Escape)
            Game.CurrentScreen = Parent;
    }

    protected virtual void OnCharacterTyped(TextInputEventArgs args) { }

    protected virtual void OnMouseClicked(PositionalMouseButtonEventArgs args)
    {
        FocusableControl? oldFocused = FocusedControl;
        FocusedControl = null;

        foreach (Control control in Controls)
            control.CheckMouseClicked(args);

        if (FocusedControl != oldFocused)
        {
            oldFocused?.InvokeFocusLost();
            FocusedControl?.InvokeFocusGained();
        }
    }

    protected virtual void OnMouseDragged(PositionalMouseButton button)
    {
        FocusedControl?.InvokeMouseDragged(button);
    }

    protected virtual void OnMouseScrolled(float scroll) { }
}