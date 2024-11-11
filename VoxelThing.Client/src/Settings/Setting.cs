using System.Globalization;
using PDS;
using VoxelThing.Client.Gui;
using VoxelThing.Client.Gui.Controls;
using VoxelThing.Client.Gui.Screens;

namespace VoxelThing.Client.Settings;

public interface ISetting
{
    public string Category { get; }
    public string Name { get; }
    public string SaveName { get; }
    public string DisplayValue { get; }
    
    public StructureItem Serialize();
    public void Deserialize(StructureItem control);
    public Control GetControl(Screen screen);
    
    protected static string ToCamelCase(string value)
    {
        value = value.Replace("_", string.Empty);
        value = value.Replace(" ", string.Empty);
        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}

public abstract class Setting<T>(string category, string name, T defaultValue) : ISetting
{
    public string Category { get; } = category;
    public string Name { get; } = name;
    public string SaveName { get; }
        = $"{ISetting.ToCamelCase(category)}:{ISetting.ToCamelCase(name)}";
    public string DisplayValue => TextTransformer(Value);
    
    public T Value { get; set; } = defaultValue;
    public bool ModifiableOnDrag { get; init; } = true;
    public Func<T, string> TextTransformer { private get; init; } = SettingsManager.DefaultTextTransformer;
 
    public StructureItem Serialize() => Value is not null ? StructureItem.Serialize(Value) : EofItem.Instance;

    public void Deserialize(StructureItem structureItem)
    {
        object? deserialized = StructureItem.Deserialize(structureItem);
        if (deserialized is T t)
            Value = t;
    }

    public Control GetControl(Screen screen)
    {
        Control control = CreateControl(screen);
        if (control is Container) return control;
        
        control.OnClick += HandleClick;
        if (control is FocusableControl focusableControl)
        {
            focusableControl.OnMouseDragged += (sender, args) => HandleClick(sender, new(args));
            focusableControl.OnFocusLost += HandleClick;
        }

        return control;
    }

    protected virtual void HandleClick(object? sender, PositionalMouseButtonEventArgs e)
        => HandleClick(sender);

    protected bool Equals(Setting<T> other)
        => Category == other.Category
           && Name == other.Name
           && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override int GetHashCode() => HashCode.Combine(Category, Name);

    protected abstract Control CreateControl(Screen screen);
    protected abstract void HandleClick(object? sender);

    public static implicit operator T(Setting<T> setting) => setting.Value;
}