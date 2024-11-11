using VoxelThing.Client.Gui;
using VoxelThing.Client.Rendering;

namespace VoxelThing.Client.Assets;

public class FontManager
{
    private readonly MainRenderer renderer;
    private readonly Dictionary<string, Font> fonts = [];

    public readonly Font Normal;
    public readonly Font Shadowed;
    public readonly Font Outlined;

    public FontManager(MainRenderer renderer)
    {
        this.renderer = renderer;

        Normal = Get("normal.png");
        Shadowed = Get("shadowed.png");
        Outlined = Get("outlined.png");
    }

    public Font Get(string path)
    {
        string fullPath = Path.Combine("gui/fonts/", path);
        
        if (!fonts.TryGetValue(path, out Font? font))
            font = new(renderer, fullPath);
        
        return font;
    }
}