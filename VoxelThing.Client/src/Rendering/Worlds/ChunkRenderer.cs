using OpenTK.Mathematics;
using VoxelThing.Client.Rendering.Vertices;
using VoxelThing.Client.Worlds;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Utils;
using VoxelThing.Game.Worlds;
using VoxelThing.Game.Worlds.Chunks;

namespace VoxelThing.Client.Rendering.Worlds;

public class ChunkRenderer : IDisposable
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public Vector3i Position => new(X, Y, Z);
	public bool EmptyOpaque { get; private set; }
	public bool EmptyTranslucent { get; private set; }
    public bool NeedsUpdate;

    public Aabb Aabb => Aabb.FromExtents(
        X * Chunk.Length, Y * Chunk.Length, Z * Chunk.Length,
        Chunk.Length, Chunk.Length, Chunk.Length
    );

	public bool Empty => EmptyOpaque && EmptyTranslucent;

    private readonly World world;
	private double firstAppearance;
	private MixedBindings? opaqueBindings;
	private MixedBindings? translucentBindings;

    public ChunkRenderer(World world, int x, int y, int z)
    {
        this.world = world;
        SetPosition(x, y, z);
    }

    public int GetIntChunkDistanceSquared(Vector3i position)
        => (Position - position).EuclideanLengthSquared;

	public void SetPosition(int x, int y, int z)
    {
		X = x;
		Y = y;
		Z = z;
		NeedsUpdate = true;
		EmptyOpaque = true;
		EmptyTranslucent = true;
        firstAppearance = 0.0;
    }

    public void Render(Profiler? profiler = null)
    {
        if (!NeedsUpdate) return;
        NeedsUpdate = false;

        if (firstAppearance == 0.0)
            firstAppearance = Game.TimeElapsed;

        EmptyOpaque = true;
        EmptyTranslucent = true;

        Chunk? chunk = world.GetChunkAt(X, Y, Z);
        if (chunk is null || chunk.Empty)
            return;

        opaqueBindings ??= new(VertexLayout.Block);
        translucentBindings ??= new(VertexLayout.Block);

        WorldCache worldCache = new(world, X, Y, Z, profiler);
        
        BlockRendererArguments args = new()
        {
            OpaqueBindings = opaqueBindings,
            TranslucentBindings = translucentBindings,
            BlockAccess = worldCache
        };

        profiler?.Push("build-buffers");
        for (int xx = 0; xx < Chunk.Length; xx++)
        {
            args.X = (X << Chunk.LengthPow2) + xx;
            for (int yy = 0; yy < Chunk.Length; yy++)
            {
                args.Y = (Y << Chunk.LengthPow2) + yy;
                for (int zz = 0; zz < Chunk.Length; zz++)
                {
                    args.Z = (Z << Chunk.LengthPow2) + zz;
                    BlockRenderer.Render(args);
                }
            }
        }
        
        EmptyOpaque = opaqueBindings.IsEmpty;
        EmptyTranslucent = translucentBindings.IsEmpty;

        profiler?.PopPush("upload-buffers");
        
        if (!EmptyOpaque) opaqueBindings.Upload(true);
        else
        {
            opaqueBindings.Dispose();
            opaqueBindings = null;
        }

        if (!EmptyTranslucent) translucentBindings.Upload(true);
        else
        {
            translucentBindings.Dispose();
            translucentBindings = null;
        }
        
        profiler?.Pop();
    }

    public void DrawOpaque()
    {
        if (EmptyOpaque || opaqueBindings is null) return;
        opaqueBindings.Draw();
    }

    public void DrawTranslucent()
    {
        if (EmptyTranslucent || translucentBindings is null) return;
        translucentBindings.Draw();
    }

    public double GetFadeAmount(double currentTime)
        => 1.0 - Math.Pow(1.0 - Math.Clamp(1.0 - (currentTime - firstAppearance), 0.0, 1.0), 2.0);

    public bool IsInCamera(Camera camera) => camera.Frustum.TestAabb(Aabb.Offset(-camera.Position));

    public void Dispose()
    {
        opaqueBindings?.Dispose();
        opaqueBindings = null;

        translucentBindings?.Dispose();
        translucentBindings = null;

        GC.SuppressFinalize(this);
    }

    public class Comparer(Vector3i cameraPosition) : IComparer<ChunkRenderer>
    {
        public Vector3i CameraPosition = cameraPosition;
        
        public int Compare(ChunkRenderer? x, ChunkRenderer? y)
            => (x?.GetIntChunkDistanceSquared(CameraPosition) ?? int.MaxValue)
                .CompareTo(y?.GetIntChunkDistanceSquared(CameraPosition) ?? int.MaxValue);
    }
}