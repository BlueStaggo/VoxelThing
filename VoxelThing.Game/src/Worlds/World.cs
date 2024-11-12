using System.Diagnostics;
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
		=> GetChunkAt(x >> Chunk.SizePow2, y >> Chunk.SizePow2, z >> Chunk.SizePow2);

	public bool ChunkExistsAtBlock(int x, int y, int z)
		=> ChunkExists(x >> Chunk.SizePow2, y >> Chunk.SizePow2, z >> Chunk.SizePow2);

	public Block? GetBlock(int x, int y, int z) => GetChunkAtBlock(x, y, z)?
        .GetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask);

	public void SetBlock(int x, int y, int z, Block? block)
    {
		Chunk? chunk = GetChunkAtBlock(x, y, z);
		if (chunk is null) return;

		chunk.SetBlock(x & Chunk.LengthMask, y & Chunk.LengthMask, z & Chunk.LengthMask, block);
		OnBlockUpdate(x, y, z);
	}

	public void GenerateChunk(Chunk chunk)
    {
        int cx = chunk.X;
        int cy = chunk.Y;
        int cz = chunk.Z;

		GenerationInfo genInfo = genCache.GetGenerationAt(cx, cz);

		genInfo.Generate();

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
						block = Block.Grass;
					} else if (yy < 0) {
						block = Block.Water;
					}
				}

				if (block is not null)
					chunk.SetBlock(x, y, z, block);
			}
		}
	}

	public List<Aabb> GetSurroundingCollision(Aabb box)
    {
		List<Aabb> boxes = [];

		int minX = (int)Math.Floor(box.MinX);
		int minY = (int)Math.Floor(box.MinY);
		int minZ = (int)Math.Floor(box.MinZ);
		int maxX = (int)Math.Floor(box.MaxX + 1.0);
		int maxY = (int)Math.Floor(box.MaxY + 1.0);
		int maxZ = (int)Math.Floor(box.MaxZ + 1.0);

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

			position += direction;
			distance += stepDistance;
		}

		return BlockRaycastResult.NoHit;
	}

	public virtual void OnBlockUpdate(int x, int y, int z) { }

	public virtual void OnChunkAdded(int x, int y, int z) { }

	public void Close()
	{
		SaveHandler.SaveData("world", (CompoundItem)Info.Serialize());
		ChunkStorage.UnloadAllChunks();
	}
}