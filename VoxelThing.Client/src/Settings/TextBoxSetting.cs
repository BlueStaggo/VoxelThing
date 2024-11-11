using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Settings;

public class TextBoxSetting(string category, string name, string defaultValue)
    : Setting<string>(category, name, defaultValue)
{
    protected override Control CreateControl(Screen screen)
        => new TextBox(screen)
        {
            Text = Value,
            AlignPosition = (0.0f, 0.0f),
            AlignSize = (1.0f, 1.0f)
        };

    protected override void HandleClick(object? sender)
    {
        if (sender is not TextBox textBox) return;
        Value = textBox.Text;
    }
}