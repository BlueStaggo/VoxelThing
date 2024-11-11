using StbImageSharp;
using VoxelThing.Client.Rendering.Textures;

namespace VoxelThing.Client.Assets;

public class TextureManager : IDisposable
{
    private readonly Dictionary<string, Texture> textures = [];
    private Texture? missingTexture = null;

    public Texture Get(string path, TextureFlags flags = TextureFlags.None)
    {
        string id = ((uint)flags) + path;

        if (textures.TryGetValue(id, out Texture? existingTexture))
            return existingTexture;
        
        try
        {
            Texture texture = LoadTexture(path, flags);
            textures[id] = texture;
            return texture;
        }
        catch (Exception)
        {
            Console.Error.WriteLine($"Warning: Texture \"{path}\" not found");
            missingTexture ??= GenerateMissingTexture();
            textures[id] = missingTexture;
            return missingTexture;
        }
    }

    public void Clear()
    {
        foreach (Texture texture in textures.Values)
            texture.Dispose();
        textures.Clear();
    }

    private static Texture LoadTexture(string path, TextureFlags flags)
    {
        string fullPath = Path.Combine(Game.AssetsDirectory, path);
        ImageResult image = ImageResult.FromStream(File.OpenRead(fullPath),
            (flags & TextureFlags.NoAlpha) != 0 ? ColorComponents.RedGreenBlue : ColorComponents.RedGreenBlueAlpha);
        return new(image.Data, image.Width, image.Height, flags);
    }

    private static Texture GenerateMissingTexture()
    {
        const int size = 16;
        const int checkerSize = 8;
        const byte colorR = 255;
        const byte colorG = 0;
        const byte colorB = 255;
        const byte altColorR = 0;
        const byte altColorG = 0;
        const byte altColorB = 0;

        byte[] data = new byte[size * size * 3];

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            int i = (x + (size - y - 1) * size) * 3;
            bool alt = (x / checkerSize + y / checkerSize) % 2 == 1;
            data[i + 0] = alt ? altColorR : colorR;
            data[i + 1] = alt ? altColorG : colorG;
            data[i + 2] = alt ? altColorB : colorB;
        }
        
        return new Texture(data, size, size, TextureFlags.NoAlpha);
    }

    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }
}