using System.Collections.Immutable;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelThing.Client.Gui;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Settings;

public class ChoiceSetting : Setting<int>
{
    public readonly ImmutableArray<string> Choices;

    public ChoiceSetting(string category, string name, int defaultValue, params string[] choices) : base(category, name, defaultValue)
    {
        Debug.Assert(choices.Length > 0, "Choices are empty!");
        Choices = [..choices];
        TextTransformer = value => value < 0 || value >= Choices.Length ? "Invalid " + value : Choices[(int)value];
    }

    protected override Control CreateControl(Screen screen)
        => new Label(screen)
        {
            Text = DisplayValue,
            Size = (100, 0),
            AlignPosition = (0.0f, 0.0f),
            AlignSize = (0.0f, 1.0f),
            HasBackground = true
        };

    protected override void HandleClick(object? sender, PositionalMouseButtonEventArgs e)
        => HandleClick(sender, e.Button == MouseButton.Button2);

    protected override void HandleClick(object? sender)
        => HandleClick(sender, false);

    private void HandleClick(object? sender, bool backwards)
    {
        if (backwards) Value--;
        else Value++;
        Value = MathUtil.FloorMod(Value, Choices.Length);

        if (sender is Label label)
            label.Text = DisplayValue;
    }
}