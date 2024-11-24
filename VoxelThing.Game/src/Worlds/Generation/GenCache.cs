using OpenTK.Mathematics;

namespace VoxelThing.Game.Worlds.Generation;

public class GenCache(World world)
{
	public const int MaxCapacity = 100;
	public const int CapacityReduction = 20;

	private readonly Dictionary<Vector2i, GenerationInfo> cache = new(MaxCapacity);
	
	public GenerationInfo GetGenerationAt(int x, int z)
	{
		Vector2i key = new(x, z);
		if (cache.TryGetValue(key, out GenerationInfo? entry) && entry.ChunkX == x && entry.ChunkZ == z)
			return entry;

		if (cache.Count > MaxCapacity)
		{
			List<Vector2i> keysToRemove = [..cache.Keys.Take(CapacityReduction)];
			foreach (Vector2i keyToRemove in keysToRemove)
				cache.Remove(keyToRemove);
		}

		entry = new GenerationInfo(world.Info.Seed, x, z);
		cache[key] = entry;
		return entry;
	}
}
