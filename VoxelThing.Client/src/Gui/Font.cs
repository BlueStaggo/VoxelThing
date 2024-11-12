using StbImageSharp;
using VoxelThing.Client.Rendering;
using VoxelThing.Client.Rendering.Drawing;
using VoxelThing.Client.Rendering.Textures;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Gui;

public class Font
{
    public readonly int LineHeight;
    public readonly int CharacterSpacing;
    public readonly int SpaceWidth;

    private readonly MainRenderer renderer;
    private readonly string texturePath;

    private readonly int[] characterWidths = new int[256];

    public Font(MainRenderer renderer, string texturePath)
    {
        this.renderer = renderer;
        this.texturePath = texturePath;

        ImageResult image = ImageResult.FromStream(
	        File.OpenRead(Path.Combine(Game.AssetsDirectory, texturePath)), ColorComponents.RedGreenBlueAlpha);
        if (image.Width == 0 || image.Height == 0)
            throw new IOException("Invalid font texture!");
        
        uint code = image.GetRgba(0, 0);
        LineHeight = (int)((code & 0xF0000000) >> 28);
        CharacterSpacing = (int)((code & 0xF000000) >> 24) - 8;
        SpaceWidth = (int)((code & 0xF00000) >> 20);

        for (int x = 0; x < 16; x++)
        for (int y = 0; y < 16; y++)
        {
            int ci = x + y * 16;
            if (ci == ' ')
            {
                characterWidths[ci] = SpaceWidth;
                continue;
            }

            int characterWidth = 0;
            for (int sx = 0; sx < 16; sx++)
                for (int sy = 0; sy <= 16; sy++)
                {
                    if (sy == 16)
                        goto breakLoopWidth;
                    
                    code = image.GetRgba(x * 16 + sx, y * 16 + sy);
                    if ((code & 0xFF) != 0)
                    {
                        characterWidth++;
                        sy = 17;
                    }
                }
            breakLoopWidth:
            characterWidths[ci] = characterWidth;
            // Console.WriteLine($"{texturePath} width of {ci} = {characterWidths}");
        }
    }

    public void Print(string text, float x, float y, float r = 1.0f, float g = 1.0f, float b = 1.0f, float scale = 1.0f, float align = 0.0f)
    {
        if (align != 0.0f)
            x -= GetStringLength(text) * scale * align;

        Draw2D draw2D = renderer.Draw2D;
        renderer.Textures.Get(texturePath).Use();

        float modR = 1.0f;
        float modG = 1.0f;
        float modB = 1.0f;
		float ox = x;

		for (int i = 0; i < text.Length; i++)
        {
			char c = text[i];
			if (c == '\u00a7' && i < text.Length + 1)
            {
				c = char.ToLower(text[++i]);
				if (c == 'c' && text.Length - i > 6)
                {
					int cr = MathUtil.HexValue(text[++i]);
					cr = (cr << 4) + MathUtil.HexValue(text[++i]);
					int cg = MathUtil.HexValue(text[++i]);
					cg = (cg << 4) + MathUtil.HexValue(text[++i]);
					int cb = MathUtil.HexValue(text[++i]);
					cb = (cb << 4) + MathUtil.HexValue(text[++i]);
					modR = cr / 255.0f;
					modG = cg / 255.0f;
					modB = cb / 255.0f;
					continue;
				}
			}

			if (c == '\n')
            {
				x = ox;
				y += LineHeight;
				continue;
			}

			if (c > 255 || c == 0)
				continue;

			int w = characterWidths[c];
			if (c != ' ') {
				float uMin = c % 16 / 16.0f;
				float vMin = c / 16 / 16.0f;
				float uMax = uMin + w / 256.0f;
				float vMax = vMin + 1.0f / 16.0f;

				float fixedX = renderer.ScreenDimensions.GetFixedPosition(x);
				float fixedY = renderer.ScreenDimensions.GetFixedPosition(y);

				draw2D.AddVertex(
                    fixedX,
                    fixedY,
                    r * modR,
                    g * modG,
                    b * modB,
                    uMin,
                    vMin
				);
				draw2D.AddVertex(
                    fixedX,
                    fixedY + scale * 16,
                    r * modR,
                    g * modG,
                    b * modB,
                    uMin,
                    vMax
				);
				draw2D.AddVertex(
                    fixedX + scale * w,
                    fixedY + scale * 16,
                    r * modR,
                    g * modG,
                    b * modB,
                    uMax,
                    vMax
				);
				draw2D.AddVertex(
                    fixedX + scale * w,
                    fixedY,
                    r * modR,
                    g * modG,
                    b * modB,
                    uMax,
                    vMin
				);
				draw2D.AddQuadIndices();
			}

			x += (w + CharacterSpacing) * scale;
		}

		draw2D.Draw();
		Texture.Stop();
    }

	public int GetStringLength(string text) {
		int x = 0;

		for (int i = 0; i < text.Length; i++)
        {
			char c = text[i];
			if (c == '\u00a7' && i < text.Length + 1)
            {
				c = char.ToLower(text[++i]);
				if (c == 'c' && text.Length - i > 6)
                {
					i += 6;
					continue;
				}
			}

			if (c > 255 || c == 0 || c == '\n')
				continue;

			x += characterWidths[c] + CharacterSpacing;
		}

		if (x > 0)
			x -= CharacterSpacing;

		return x;
	}
}