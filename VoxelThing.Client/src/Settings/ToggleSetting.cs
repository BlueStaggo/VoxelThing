using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Settings;

public class ToggleSetting(string category, string name, bool defaultValue)
    : Setting<bool>(category, name, defaultValue)
{
    protected override Control CreateControl(Screen screen)
        => new Label(screen)
        {
            Text = DisplayValue,
            Size = (100, 0),
            AlignPosition = (0.0f, 0.0f),
            AlignSize = (0.0f, 1.0f),
            HasBackground = true
        };

    protected override void HandleClick(object? sender)
    {
        Value = !Value;
        if (sender is Label label)
            label.Text = DisplayValue;
    }
}