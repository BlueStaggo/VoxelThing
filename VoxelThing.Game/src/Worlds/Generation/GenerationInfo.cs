using VoxelThing.Game.Worlds.Chunks;
using VoxelThing.Game.Maths;

namespace VoxelThing.Game.Worlds.Generation;

public class GenerationInfo
{
    private const int LerpMapLength = (Chunk.Length >> 2) + 1;

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
    private int lastQueryLayer = int.MaxValue;
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
		}
	}

    public float GetHeight(int x, int z) => height[x, z];

    public bool GetCave(int x, int y, int z)
    {
        if (lastQueryLayer != y >> Chunk.SizePow2)
            GenerateCaves(y >> Chunk.SizePow2);
        
		int xx = x / 4;
		int yy = (y & Chunk.LengthMask) / 4;
		int zz = z / 4;

		float c000 = caveInfo[xx, yy, zz];
		float c001 = caveInfo[xx, yy, zz + 1];
		float c010 = caveInfo[xx, yy + 1, zz];
		float c011 = caveInfo[xx, yy + 1, zz + 1];
		float c100 = caveInfo[xx + 1, yy, zz];
		float c101 = caveInfo[xx + 1, yy, zz + 1];
		float c110 = caveInfo[xx + 1, yy + 1, zz];
		float c111 = caveInfo[xx + 1, yy + 1, zz + 1];

		float lerpedCaveInfo = MathUtil.TrilinearInterpolation(c000, c001, c010, c011, c100, c101, c110, c111, (x & 3) / 4.0f, (y & 3) / 4.0f, (z & 3) / 4.0f);
		float cheeseThreshold = Math.Clamp(-y / CheeseDensitySpread + CheeseDensitySurface, CheeseMinDensity, CheeseMaxDensity);
		return lerpedCaveInfo < cheeseThreshold;
    }

	private void GenerateCaves(int layer)
    {
		lastQueryLayer = layer;
		Array.Clear(caveInfo);

		for (int x = 0; x < LerpMapLength; x++)
		for (int y = 0; y < LerpMapLength; y++)
		for (int z = 0; z < LerpMapLength; z++)
        {
			int xx = (x << 2) + (ChunkX << Chunk.SizePow2);
			int yy = (y << 2) + (layer << Chunk.SizePow2);
			int zz = (z << 2) + (ChunkZ << Chunk.SizePow2);

			float cheese = caveNoise.Noise3_ImproveXZ(xx / CheeseScaleXz, yy / CheeseScaleY, zz / CheeseScaleXz);
			caveInfo[x, y, z] = cheese;
		}
	}
}