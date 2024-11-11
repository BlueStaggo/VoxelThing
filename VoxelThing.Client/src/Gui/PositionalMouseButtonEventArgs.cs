using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelThing.Client.Gui;

public readonly struct PositionalMouseButtonEventArgs(MouseButtonEventArgs mouse, Vector2i position)
{
    public PositionalMouseButtonEventArgs(PositionalMouseButton positionalMouseButton)
        : this(new(positionalMouseButton.Button, InputAction.Repeat, 0), positionalMouseButton.Position) { }
    
    public MouseButtonEventArgs Mouse { get; init; } = mouse;
    public Vector2i Position { get; init; } = position;
    
    public MouseButton Button => Mouse.Button;
    public InputAction Action => Mouse.Action;
    public KeyModifiers Modifiers => Mouse.Modifiers;
    public bool IsPressed => Mouse.IsPressed;
}

public readonly struct PositionalMouseButton(MouseButton mouseButton, Vector2i position)
{
    public MouseButton Button { get; init; } = mouseButton;
    public Vector2i Position { get; init; } = position;
}