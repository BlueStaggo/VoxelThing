namespace VoxelThing.Client.Rendering.Textures;

[Flags]
public enum TextureFlags
{
    None = 0,
    Mipmapped = 1,
    NoAlpha = 2,
    Bilinear = 4,
}