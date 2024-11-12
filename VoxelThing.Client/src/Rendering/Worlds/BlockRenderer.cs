using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Game;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Utils;
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

    public static bool Render(BlockRendererArguments args, Profiler? profiler = null)
    {
	    profiler?.Push("block-access");
        Block? block = args.Chunk.GetBlock(args.X, args.Y, args.Z);
        profiler?.Pop();
        if (block is null) return false;

        int xx = args.Chunk.ToGlobalX(args.X);
        int yy = args.Chunk.ToGlobalY(args.Y);
        int zz = args.Chunk.ToGlobalZ(args.Z);

        profiler?.Push("render-faces");
		FaceRenderingArguments fargs = new(args, block);
        for (Direction direction = 0; (int)direction < 6; direction++)
	        if (block.IsFaceDrawn(
	            args.BlockAccess,
	            xx + direction.GetX(),
	            yy + direction.GetY(),
	            zz + direction.GetZ(),
	            direction
	        ))
                SideRenderers[(int)direction](fargs);
        profiler?.Pop();

        return true;
    }

	private static void RenderNorthFace(FaceRenderingArguments args)
    {
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		int x = args.X;
        int y = args.Y;
        int z = args.Z;
		
        int gx = args.Chunk.ToGlobalX(x);
        int gy = args.Chunk.ToGlobalY(y);
        int gz = args.Chunk.ToGlobalZ(z);

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
		public readonly Chunk Chunk = baseArgs.Chunk;
		public readonly int X = baseArgs.X;
		public readonly int Y = baseArgs.Y;
		public readonly int Z = baseArgs.Z;
	}
}
