using OpenTK.Mathematics;

namespace VoxelThing.Client.Gui;

public class ScreenDimensions
{
    public const float MaxDepth = 100.0f;
    public const int VirtualWidth = 427;
    public const int VirtualHeight = 240;

    public float ManualScale;
    public float Scale { get; private set; }
    public Matrix4 ViewProj => viewProj;

    public float Width { get; private set; } = -1.0f;
    public float Height { get; private set; } = -1.0f;
    public Vector2 Size => new(Width, Height);

    public int IntWidth => (int)MathF.Ceiling(Width);
    public int IntHeight => (int)MathF.Ceiling(Height);
    public Vector2i IntSize => new(IntWidth, IntHeight);
    public Vector2i ScaledMousePosition => (Vector2i)(game.MousePosition / Scale);

    private readonly Game game;
    private Matrix4 viewProj;

    public ScreenDimensions(Game game)
    {
        this.game = game;
        UpdateDimensions();
    }

	public void UpdateDimensions()
    {
		float newWidth = game.ClientSize.X;
		float newHeight = game.ClientSize.Y;

		Scale = 1.0f;
		while (newWidth / (Scale + 1) >= VirtualWidth && newHeight / (Scale + 1) >= VirtualHeight)
			Scale++;

		if (ManualScale > 0.0f)
			Scale = Math.Min(Scale, ManualScale);

		newWidth /= Scale;
		newHeight /= Scale;

		if (Math.Abs(Width - newWidth) > 1.0e-3f || Math.Abs(Height - newHeight) > 1.0e-3f)
        {
			Width = newWidth;
			Height = newHeight;
            Matrix4.CreateOrthographicOffCenter(0.0f, Width, Height, 0.0f, 0.0f, MaxDepth, out viewProj);
		}
	}

	public float GetFixedPosition(float x) => (int)(x * Scale) / Scale;
}