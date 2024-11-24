using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Generation;

public class GenerationInfo
{
	private const int LerpLengthPow2 = 2;
	private const int LerpLength = 1 << LerpLengthPow2;
	private const int LerpMapLength = (Chunk.Length >> LerpLengthPow2) + 1;

    private const int BaseOctaves = 4;
    private const double BaseScale = 250.0;
    private const float BaseHeightScale = 8.0f;

    private const int HillOctaves = 3;
    private const double HillScale = 250.0;
    private const float HillHeightScale = 16.0f;
    private const float HillHeightScaleMod = 4.0f;
    private const float HillThresholdMin = -0.5f;
    private const float HillThresholdMax = 1.0f;

    private const int CliffOctaves = 2;
    private const double CliffScale = 250.0;
    private const float CliffThreshold = 0.5f;

    private const int CliffHeightOctaves = 1;
    private const double CliffHeightScale = 100.0;
    private const float CliffHeightMin = 2.0f;
    private const float CliffHeightMax = 8.0f;

	private const float CheeseMinDensity = -1.0f;
	private const float CheeseMaxDensity = -0.3f;
	private const float CheeseDensitySpread = 100.0f;
	private const float CheeseDensitySurface = -0.5f;
    private const int CheeseOctaves = 4;
    private const double CheeseScaleXz = 100.0;
    private const double CheeseScaleY = 50.0;

    public readonly int ChunkX, ChunkZ;

    private OpenSimplex2Octaves baseNoise;
    private OpenSimplex2Octaves hillNoise;
    private OpenSimplex2Octaves cliffNoise;
    private OpenSimplex2Octaves cliffHeightNoise;
    private OpenSimplex2Octaves caveNoise;

    private readonly float[,] height = new float[Chunk.Length, Chunk.Length];
    private readonly float[,,] caveInfo = new float[LerpMapLength, LerpMapLength, LerpMapLength];
    private readonly float[,,] lerpedCaveInfo = new float[Chunk.Length, Chunk.Length, Chunk.Length];
    private float maxHeight = float.NegativeInfinity;
    private int caveChunkY = int.MaxValue;
    private bool hasGenerated;

    public GenerationInfo(ulong seed, int cx, int cz)
    {
        Random64 random = new(seed);

		baseNoise = new(random, BaseOctaves);
		hillNoise = new(random, HillOctaves);
		cliffNoise = new(random, CliffOctaves);
		cliffHeightNoise = new(random, CliffHeightOctaves);
		caveNoise = new(random, CheeseOctaves);

		ChunkX = cx;
		ChunkZ = cz;
    }

	public void Generate()
    {
		if (hasGenerated) return;
		hasGenerated = true;

		for (int x = 0; x < Chunk.Length; x++)
		for (int z = 0; z < Chunk.Length; z++)
        {
			int xx = (ChunkX * Chunk.Length + x);
			int zz = (ChunkZ * Chunk.Length + z);

			float baseHeight = baseNoise.Noise2(xx / BaseScale, zz / BaseScale);
			float hill = hillNoise.Noise2(xx / HillScale, zz / HillScale);
			hill = 1.0f - MathF.Cos(MathUtil.Threshold(hill, HillThresholdMin, HillThresholdMax) * MathF.PI / 2.0f);

			float addedBaseHeight = BaseHeightScale * float.Lerp(1.0f, HillHeightScaleMod, hill);
			baseHeight = baseHeight * addedBaseHeight + hill * HillHeightScale;

			float cliff = cliffNoise.Noise2(xx / CliffScale, zz / CliffScale);
			float cliffHeight = cliffHeightNoise.Noise2(xx / CliffHeightScale, zz / CliffHeightScale);
			cliffHeight = float.Lerp(CliffHeightMin, CliffHeightMax, cliffHeight / 2.0f + 0.5f) * (1.0f - hill * 5.0f);

			if (cliff > CliffThreshold)
				baseHeight += cliffHeight;

			height[x, z] = baseHeight;
			if (baseHeight > maxHeight)
				maxHeight = baseHeight;
        }
	}

    public float GetHeight(int x, int z) => height[x, z];

    public bool GetCave(int x, int y, int z)
    {
	    if (caveChunkY << Chunk.LengthPow2 > Math.Ceiling(maxHeight / Chunk.Length)) return false;

	    float cheese = lerpedCaveInfo[x, y & Chunk.LengthMask, z];
		float cheeseThreshold = Math.Clamp(-y / CheeseDensitySpread + CheeseDensitySurface, CheeseMinDensity, CheeseMaxDensity);
		return cheese < cheeseThreshold;
    }

	public void GenerateCaves(int chunkY)
	{
		if (caveChunkY == chunkY) return;
		
		caveChunkY = chunkY;
		Array.Clear(caveInfo);
		Array.Clear(lerpedCaveInfo);

		if (chunkY > (int)Math.Ceiling(maxHeight / Chunk.Length)) return;

		for (int x = 0; x < LerpMapLength; x++)
		for (int y = 0; y < LerpMapLength; y++)
		for (int z = 0; z < LerpMapLength; z++)
        {
			int xx = (x << LerpLengthPow2) + (ChunkX << Chunk.LengthPow2);
			int yy = (y << LerpLengthPow2) + (chunkY << Chunk.LengthPow2);
			int zz = (z << LerpLengthPow2) + (ChunkZ << Chunk.LengthPow2);

			float cheese = caveNoise.Noise3_ImproveXZ(xx / CheeseScaleXz, yy / CheeseScaleY, zz / CheeseScaleXz);
			caveInfo[x, y, z] = cheese;
		}

		MathUtil.TrilinearInterpolation(caveInfo, lerpedCaveInfo, LerpLength, LerpLength, LerpLength);
	}
}