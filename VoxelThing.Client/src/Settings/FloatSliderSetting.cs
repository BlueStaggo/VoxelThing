using System.Numerics;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Settings;

public class FloatSliderSetting<T>(string category, string name, T defaultValue, T minimum, T maximum, T multiple)
    : Setting<T>(category, name, defaultValue)
    where T : IFloatingPoint<T>
{
    public readonly T Minimum = minimum;
    public readonly T Maximum = maximum;
    public readonly T Range = maximum - minimum;
    public readonly T Multiple = multiple;

    public FloatSliderSetting(string category, string name, T defaultValue, T minimum, T maximum)
        : this(category, name, defaultValue, minimum, maximum, T.Zero) { }

    protected override Control CreateControl(Screen screen)
        => new Slider(screen)
        {
            Text = DisplayValue,
            Value = float.CreateTruncating((Value - Minimum) / Range),
            AlignPosition = (0.0f, 0.0f),
            AlignSize = (1.0f, 1.0f)
        };

    protected override void HandleClick(object? sender)
    {
        if (sender is not Slider slider) return;

        T value = T.CreateTruncating(slider.Value);
        value = (value + Minimum) * Range;
        if (Multiple > T.Zero)
            value = T.Round(value / Multiple) * Multiple;
        Value = value;
        
        slider.Text = DisplayValue;
    }
}