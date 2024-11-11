using System.Numerics;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Settings;

public class IntSliderSetting<T>(string category, string name, T defaultValue, T minimum, T maximum, T multiple)
    : Setting<T>(category, name, defaultValue)
    where T : IBinaryInteger<T>
{
    public readonly T Minimum = minimum;
    public readonly T Maximum = maximum;
    public readonly T Range = maximum - minimum;
    public readonly T Multiple = multiple;

    private readonly float floatMinimum = float.CreateTruncating(minimum); 
    private readonly float floatRange = float.CreateTruncating(maximum - minimum); 

    public IntSliderSetting(string category, string name, T defaultValue, T minimum, T maximum)
        : this(category, name, defaultValue, minimum, maximum, T.Zero) { }

    protected override Control CreateControl(Screen screen)
        => new Slider(screen)
        {
            Text = DisplayValue,
            Value = (float.CreateTruncating(Value) - floatMinimum) / floatRange,
            AlignPosition = (0.0f, 0.0f),
            AlignSize = (1.0f, 1.0f)
        };

    protected override void HandleClick(object? sender)
    {
        if (sender is not Slider slider) return;

        slider.Value = MathF.Round(slider.Value * floatRange) / floatRange;
        T value = T.CreateTruncating(slider.Value * floatRange + floatMinimum);
        if (Multiple > T.Zero)
            value = value / Multiple * Multiple;
        Value = value;
        
        slider.Text = DisplayValue;
    }
}