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

    public readonly Setting<int> FpsLimit;
    public readonly Setting<int> VSync;
    public readonly Setting<int> RenderDistanceHorizontal;
    public readonly Setting<int> RenderDistanceVertical;
    public readonly Setting<bool> ViewBobbing;
    public readonly Setting<int> FieldOfView;
    public readonly Setting<bool> ThirdPerson;
    public readonly Setting<bool> HideHud;
    public readonly Setting<bool> Mipmaps;
    public readonly Setting<int> Skin;
    public readonly Setting<int> GuiScale;
    public readonly Setting<float> MouseSensitivity;
    public readonly Setting<float> HorizonR, HorizonG, HorizonB;
    public readonly Setting<float> SkyR, SkyG, SkyB;
    public readonly Setting<float> FogR, FogG, FogB;

    public SettingsManager()
    {
        Categories = categoriesMutable.AsReadOnly();
        
        FpsLimit = Add(new IntSliderSetting<int>("Graphics", "FPS Limit", 120, 0, 250, 5)
            { TextTransformer = fps => fps == 0 ? "None" : fps.ToString() });
        VSync = Add(new ChoiceSetting("Graphics", "VSync", 0, "OFF", "ON", "Adaptive"));
        RenderDistanceHorizontal = Add(new IntSliderSetting<int>("Graphics", "Horizontal Render Distance", 12, 1, 16));
        RenderDistanceVertical = Add(new IntSliderSetting<int>("Graphics", "Vertical Render Distance", 6, 1, 16));
        ViewBobbing = Add(new ToggleSetting("Graphics", "View Bobbing", true));
        FieldOfView = Add(new IntSliderSetting<int>("Graphics", "Field of View", 70, 30, 120));
        ThirdPerson = Add(new ToggleSetting("Graphics", "Third Person", false));
        HideHud = Add(new ToggleSetting("Graphics", "Hide Hud", false));
        Mipmaps = Add(new ToggleSetting("Graphics", "Mipmaps", true));
        Skin = Add(new ChoiceSetting("Graphics", "Skin", 0, Game.Skins
            .Select(skin => (char.ToUpper(skin[0]) + skin[1..]).Replace('_', ' '))
            .ToArray()));
        GuiScale = Add(new IntSliderSetting<int>("Graphics", "Gui Scale", 0, 0, 4) { ModifiableOnDrag = false });
        
        MouseSensitivity = Add(new FloatSliderSetting<float>("Controls", "Mouse Sensitivity", 0.2f, 0.0f, 1.0f));
        
        HorizonR = Add(new FloatSliderSetting<float>("Fun", "Horizon R", 0.7f, 0.0f, 1.0f, 0.05f));
        HorizonG = Add(new FloatSliderSetting<float>("Fun", "Horizon G", 0.9f, 0.0f, 1.0f, 0.05f));
        HorizonB = Add(new FloatSliderSetting<float>("Fun", "Horizon B", 1.0f, 0.0f, 1.0f, 0.05f));
        SkyR = Add(new FloatSliderSetting<float>("Fun", "Sky R", 0.1f, 0.0f, 1.0f, 0.05f));
        SkyG = Add(new FloatSliderSetting<float>("Fun", "Sky G", 0.4f, 0.0f, 1.0f, 0.05f));
        SkyB = Add(new FloatSliderSetting<float>("Fun", "Sky B", 1.0f, 0.0f, 1.0f, 0.05f));
        FogR = Add(new FloatSliderSetting<float>("Fun", "Fog R", 1.0f, 0.0f, 1.0f, 0.05f));
        FogG = Add(new FloatSliderSetting<float>("Fun", "Fog G", 1.0f, 0.0f, 1.0f, 0.05f));
        FogB = Add(new FloatSliderSetting<float>("Fun", "Fog B", 1.0f, 0.0f, 1.0f, 0.05f));
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