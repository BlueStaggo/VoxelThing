using OpenTK.Windowing.Common;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Gui.Controls;

public class FocusableControl : Control
{
    public event EventHandler<KeyboardKeyEventArgs>? OnKeyPressed;
    public event EventHandler<TextInputEventArgs>? OnCharacterTyped;
    public event EventHandler<PositionalMouseButton>? OnMouseDragged;
    public event Action<object>? OnFocusGained;
    public event Action<object>? OnFocusLost;

    public virtual bool DragFocusOnly => false;
    public bool Focused => this == Screen.FocusedControl;

    public FocusableControl(Screen screen) : base(screen)
    {
        OnClick += (_, _) => screen.FocusedControl = this;
    }

    public void InvokeKeyPressed(KeyboardKeyEventArgs args) => OnKeyPressed?.Invoke(this, args);

    public void InvokeCharacterTyped(TextInputEventArgs args) => OnCharacterTyped?.Invoke(this, args);

    public void InvokeMouseDragged(PositionalMouseButton button) => OnMouseDragged?.Invoke(this, button);
    
    public void InvokeFocusGained() => OnFocusGained?.Invoke(this);

    public void InvokeFocusLost() => OnFocusLost?.Invoke(this);
}