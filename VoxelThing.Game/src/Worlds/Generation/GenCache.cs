using VoxelThing.Game.Worlds.Storage;

namespace VoxelThing.Game.Worlds.Generation;

public class GenCache(World world)
{
	public const int DiameterPow2 = ChunkStorage.DiameterPow2;
	public const int Diameter = ChunkStorage.Diameter;
	public const int DiameterMask = ChunkStorage.DiameterMask;

	private readonly GenerationInfo?[,] cache = new GenerationInfo?[Diameter, Diameter];
	
	public GenerationInfo GetGenerationAt(int x, int z)
	{
		GenerationInfo? entry = cache[x & DiameterMask, z & DiameterMask];
		if (entry is not null && entry.ChunkX == x && entry.ChunkZ == z)
			return entry;

		entry = new GenerationInfo(world.Info.Seed, x, z);
		cache[x & DiameterMask, z & DiameterMask] = entry;
		return entry;
	}
}
