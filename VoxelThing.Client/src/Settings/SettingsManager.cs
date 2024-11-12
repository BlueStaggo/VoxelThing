using System.Collections.ObjectModel;
using System.Globalization;
using PDS;

namespace VoxelThing.Client.Settings;

public class SettingsManager
{
    private string? savePath;

    private readonly Dictionary<string, ISetting> bySaveName = [];
    private readonly Dictionary<string, List<ISetting>> byCategory = [];
    private readonly HashSet<ISetting> settingsSet = [];
    private readonly List<string> categoriesMutable = [];

    public readonly ReadOnlyCollection<string> Categories;

    public readonly Setting<int> LimitFps;
    public readonly Setting<int> RenderDistanceHorizontal;
    public readonly Setting<int> RenderDistanceVertical;
    public readonly Setting<bool> ViewBobbing;
    public readonly Setting<bool> ThirdPerson;
    public readonly Setting<bool> HideHud;
    public readonly Setting<int> Skin;
    public readonly Setting<int> GuiScale;
    public readonly Setting<float> MouseSensitivity;

    public SettingsManager()
    {
        Categories = categoriesMutable.AsReadOnly();
        
        LimitFps = Add(new ChoiceSetting("Graphics", "Limit Fps", 2, "OFF", "ON", "Menu Only"));
        RenderDistanceHorizontal = Add(new IntSliderSetting<int>("Graphics", "Horizontal Render Distance", 16, 1, 16));
        RenderDistanceVertical = Add(new IntSliderSetting<int>("Graphics", "Vertical Render Distance", 8, 1, 16));
        ViewBobbing = Add(new ToggleSetting("Graphics", "View Bobbing", true));
        ThirdPerson = Add(new ToggleSetting("Graphics", "Third Person", false));
        HideHud = Add(new ToggleSetting("Graphics", "Hide Hud", false));
        Skin = Add(new ChoiceSetting("Graphics", "Skin", 0, Game.Skins
            .Select(skin => (char.ToUpper(skin[0]) + skin[1..]).Replace('_', ' '))
            .ToArray()));
        GuiScale = Add(new IntSliderSetting<int>("Graphics", "Gui Scale", 0, 0, 4) { ModifiableOnDrag = false });
        MouseSensitivity = Add(new FloatSliderSetting<float>("Controls", "Mouse Sensitivity", 0.25f, 0.0f, 1.0f));
    }

    private T Add<T>(T setting)
        where T : ISetting
    {
        if (!byCategory.TryGetValue(setting.Category, out var settingsList))
        {
            settingsList = [];
            byCategory[setting.Category] = settingsList;
            categoriesMutable.Add(setting.Category);
        }

        settingsSet.Add(setting);
        settingsList.Add(setting);
        bySaveName[setting.SaveName] = setting;

        return setting;
    }
    
    public void ReadFrom(string path)
    {
        savePath = path;
        StructureItem data;
        
        try { data = StructureItem.ReadFromPath(savePath); }
        catch (Exception) { return; }

        if (data is not CompoundItem compoundItem) return;

        foreach (string key in compoundItem.DictionaryValue.Keys)
        {
            if (bySaveName.TryGetValue(key, out ISetting? setting))
                setting.Deserialize(compoundItem[key] ?? EofItem.Instance);
        }
    }

    public void Save(string? path = null)
    {
        path ??= savePath;
        if (path is null) return;

        CompoundItem data = new();

        foreach (ISetting setting in settingsSet)
            data[setting.SaveName] = setting.Serialize();

        try { data.WriteToPath(path); }
        catch (Exception e)
        {
            Console.Error.WriteLine("Failed to save settings!");
            Console.Error.WriteLine(e);
        }
    }

    public ReadOnlyCollection<ISetting> GetSettingsFromCategory(string category)
        => byCategory.TryGetValue(category, out List<ISetting>? settings)
            ? settings.AsReadOnly() : ReadOnlyCollection<ISetting>.Empty;
    
    public static string DefaultTextTransformer<T>(T value)
    {
        return value switch
        {
            float f => (MathF.Floor(f * 100.0f) / 100.0f).ToString(CultureInfo.CurrentCulture),
            bool b => b ? "ON" : "OFF",
            _ => value?.ToString() ?? ""
        };
    }
    
    public static string PercentageTextTransformer(float value)
        => (int)(value * 100.0f) + "%";
}