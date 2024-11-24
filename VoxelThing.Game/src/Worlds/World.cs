using OpenTK.Mathematics;
using PDS;
using VoxelThing.Game.Blocks;
using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Worlds.Generation;
using VoxelThing.Game.Worlds.Storage;
using VoxelThing.Game.Maths;
using VoxelThing.Game.Utils;

namespace VoxelThing.Game.Worlds;

public class World : IBlockAccess
{
    public readonly ISaveHandler SaveHandler;
    public readonly WorldInfo Info;
    public readonly Random64 Random = new();

    protected readonly ChunkStorage ChunkStorage;
    private readonly GenCache genCache;

    public Profiler? Profiler { get; protected init;  }

    public World(ISaveHandler saveHandler, WorldInfo? info = null)
    {
        info ??= saveHandler.LoadData("world")?.Deserialize<WorldInfo>() ?? new();
        
        SaveHandler = saveHandler;
		Info = info;
        ChunkStorage = new(this);
        genCache = new(this);
    }
    
	public Chunk? GetChunkAt(int x, int y, int z) => ChunkStorage.GetChunkAt(x, y, z);

	public bool ChunkExists(int x, int y, int z) => ChunkStorage.ChunkExists(x, y, z);

	public Chunk? GetChunkAtBlock(int x, int y, int z)
		=> GetChunkAt(x >> Chunk.LengthPow2, y >> Chunk.LengthPow2, z >> Chunk.LengthPow2);

	public bool ChunkExistsAtBlock(int x, int y, int z)
		=> ChunkExists(x >> Chunk.LengthPow2, y >> Chunk.LengthPow2, z >> Chunk.LengthPow2);

	public Block? GetBlock(int x, int y, int z) => GetChunkAtBlock(x, y, z)?
        .GetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask);

	public void SetBlock(int x, int y, int z, Block? block)
    {
		Chunk? chunk = GetChunkAtBlock(x, y, z);
		if (chunk is null) return;

		chunk.SetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask, block);
		OnBlockUpdate(x, y, z);
	}

	public void GenerateChunk(Chunk chunk, Profiler? profiler = null)
    {
        int cx = chunk.X;
        int cy = chunk.Y;
        int cz = chunk.Z;

		GenerationInfo genInfo = genCache.GetGenerationAt(cx, cz);

		profiler?.Push("gen-noise");
		genInfo.Generate();
		profiler?.PopPush("gen-caves");
		genInfo.GenerateCaves(cy);

		profiler?.PopPush("place-blocks");
		for (int x = 0; x < Chunk.Length; x++)
		for (int z = 0; z < Chunk.Length; z++)
        {
			float height = genInfo.GetHeight(x, z);

			for (int y = 0; y < Chunk.Length; y++)
            {
				int yy = cy * Chunk.Length + y;
				bool cave = yy < height && genInfo.GetCave(x, yy, z);
				Block? block = null;

				if (!cave) {
					if (yy < height - 4) {
						block = Block.Stone;
					} else if (yy < height - 1) {
						block = Block.Dirt;
					} else if (yy < height) {
						block = height < -2 ? Block.Gravel
							: height < 1 ? Block.Sand
							: Block.Grass;
					} else if (yy < 0) {
						block = Block.Water;
					}
				}

				if (block is not null)
					chunk.SetBlock(x, y, z, block);
			}
		}
		profiler?.Pop();
	}

	public List<Aabb> GetSurroundingCollision(Aabb box)
    {
		List<Aabb> boxes = [];

		int minX = (int)Math.Floor(box.MinX - 1.0);
		int minY = (int)Math.Floor(box.MinY - 1.0);
		int minZ = (int)Math.Floor(box.MinZ - 1.0);
		int maxX = (int)Math.Floor(box.MaxX + 2.0);
		int maxY = (int)Math.Floor(box.MaxY + 2.0);
		int maxZ = (int)Math.Floor(box.MaxZ + 2.0);

		for (int x = minX; x < maxX; x++)
		for (int y = minY; y < maxY; y++)
		for (int z = minZ; z < maxZ; z++)
			if (GetBlock(x, y, z) is { } block)
				boxes.Add(block.GetCollisionBox(this, x, y, z));

		return boxes;
	}

	public BlockRaycastResult DoRaycast(Vector3d position, Vector3d direction, float length)
    {
		const float stepDistance = 1.0f / 16.0f;

		float distance = 0.0f;

		while (distance < length)
		{
			int x = (int)Math.Floor(position.X);
			int y = (int)Math.Floor(position.Y);
			int z = (int)Math.Floor(position.Z);

			Block? block = GetBlock(x, y, z);
			if (block is not null)
			{
				Aabb collision = block.GetCollisionBox(this, x, y, z);
				if (collision.Contains(position))
					return new(x, y, z, collision.GetClosestFace(position, direction));
			}

			position += direction * stepDistance;
			distance += stepDistance;
		}

		return BlockRaycastResult.NoHit;
	}

	public void UnloadSurroundingChunks(int cx, int cy, int cz, int distanceH, int distanceV)
		=> ChunkStorage.UnloadSurroundingChunks(cx, cy, cz, distanceH, distanceV);
	
	public virtual void OnBlockUpdate(int x, int y, int z) { }

	public virtual void OnChunkAdded(int x, int y, int z) { }

	public void Close()
	{
		SaveHandler.SaveData("world", (CompoundItem)StructureItem.Serialize(Info));
		ChunkStorage.UnloadAllChunks();
	}
}