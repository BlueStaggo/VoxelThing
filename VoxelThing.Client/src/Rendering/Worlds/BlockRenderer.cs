using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Rendering.Worlds;

public static class BlockRenderer
{
    private const float ShadeFactor = 0.15f;

    private static readonly SideRenderer[] SideRenderers =
    [
        RenderNorthFace,
        RenderSouthFace,
        RenderWestFace,
        RenderEastFace,
        RenderBottomFace,
        RenderTopFace,
    ];

    private delegate void SideRenderer(FaceRenderingArguments args);

    private static byte GetShade(int amount) => (byte)((1.0f - ShadeFactor * amount) * 255.0f);

    public static void Render(BlockRendererArguments args)
    {
        Block? block = args.BlockAccess.GetBlock(args.X, args.Y, args.Z);
        if (block is null)
	        return;

		FaceRenderingArguments fargs = new(args, block);
		for (Direction direction = 0; (int)direction < 6; direction++)
	        if (block.IsFaceDrawn(
	            args.BlockAccess,
	            args.X + direction.GetX(),
	            args.Y + direction.GetY(),
	            args.Z + direction.GetZ(),
	            direction
            ))
                SideRenderers[(int)direction](fargs);
    }

	private static void RenderNorthFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.North, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(1);

		AddVertices(args.Bindings,  x + 1,  y + 1,  z,  shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x + 1,  y,      z,  shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x,      y,      z,  shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x,      y + 1,  z,  shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void RenderSouthFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.South, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(1);

		AddVertices(args.Bindings,  x,      y + 1,  z + 1,  shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x,      y,      z + 1,  shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x + 1,  y,      z + 1,  shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x + 1,  y + 1,  z + 1,  shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void RenderWestFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.West, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(2);

		AddVertices(args.Bindings,  x,  y + 1,  z,      shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x,  y,      z,      shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x,  y,      z + 1,  shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x,  y + 1,  z + 1,  shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void RenderEastFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.East, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(2);

		AddVertices(args.Bindings,  x + 1,  y + 1,  z + 1,  shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x + 1,  y,      z + 1,  shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x + 1,  y,      z,      shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x + 1,  y + 1,  z,      shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void RenderBottomFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.Down, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(3);

		AddVertices(args.Bindings,  x + 1,  y,  z,      shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x + 1,  y,  z + 1,  shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x,      y,  z + 1,  shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x,      y,  z,      shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void RenderTopFace(FaceRenderingArguments args)
    {
		int gx = args.X;
        int gy = args.Y;
        int gz = args.Z;
		
        int x = gx & Chunk.LengthMask;
        int y = gy & Chunk.LengthMask;
        int z = gz & Chunk.LengthMask;

		Vector2i texture = args.Block.Texture.Get(Direction.Up, args.BlockAccess, gx, gy, gz);
		float texX = texture.X * Block.TextureResolution;
		float texY = texture.Y * Block.TextureResolution;
		float texXp = texX + Block.TextureResolution;
		float texYp = texY + Block.TextureResolution;
		byte shade = GetShade(0);

		AddVertices(args.Bindings,  x + 1,  y + 1,  z + 1,  shade,  shade,  shade,  texX,   texY    );
		AddVertices(args.Bindings,  x + 1,  y + 1,  z,      shade,  shade,  shade,  texX,   texYp   );
		AddVertices(args.Bindings,  x,      y + 1,  z,      shade,  shade,  shade,  texXp,  texYp   );
		AddVertices(args.Bindings,  x,      y + 1,  z + 1,  shade,  shade,  shade,  texXp,  texY    );
		args.Bindings.AddQuadIndices();
	}

	private static void AddVertices(MixedBindings bindings, float x, float y, float z, byte r, byte g, byte b, float u, float v)
		=> bindings.Put(x).Put(y).Put(z).Put(r).Put(g).Put(b)
			.Put(u / Block.TextureAtlasResolution).Put(v / Block.TextureAtlasResolution);

	private readonly struct FaceRenderingArguments(BlockRendererArguments baseArgs, Block block)
	{
		public readonly MixedBindings Bindings
			= block.Translucent ? baseArgs.TranslucentBindings : baseArgs.OpaqueBindings;
		public readonly Block Block = block;
		public readonly IBlockAccess BlockAccess = baseArgs.BlockAccess;
		public readonly int X = baseArgs.X;
		public readonly int Y = baseArgs.Y;
		public readonly int Z = baseArgs.Z;
	}
}
