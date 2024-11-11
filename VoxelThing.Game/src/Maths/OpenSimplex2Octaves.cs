namespace VoxelThing.Game.Maths;

public class OpenSimplex2Octaves
{
    private readonly int octaves;
    private readonly long[] seeds;
    private readonly OctaveNoiseParameters parameters;
    private readonly float topAmplitude;

    public OpenSimplex2Octaves(Random random, int octaves) : this(random, octaves, new()) { }

    public OpenSimplex2Octaves(Random random, int octaves, OctaveNoiseParameters parameters)
    {
        this.octaves = octaves;
        seeds = Enumerable.Range(0, octaves).Select(_ => random.NextInt64()).ToArray();
        this.parameters = parameters;

        float amplitude = 1.0f;
        float amplitudeAdd = 1.0f;

        for (int i = 1; i < octaves; i++)
        {
            amplitudeAdd *= parameters.Persistence;
            amplitude += amplitudeAdd;
        }

        topAmplitude = amplitude;
    }

    public float Noise2(double x, double y)
    {
        float value = 0.0f;
        double frequency = 1.0;
        float amplitude = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            value += OpenSimplex2.Noise2(seeds[i], x * frequency, y * frequency) * amplitude;
            frequency *= parameters.Lacunarity;
            amplitude *= parameters.Persistence;
        }

        return value / topAmplitude;
    }

    public float Noise3_ImproveXY(double x, double y, double z)
    {
        float value = 0.0f;
        double frequency = 1.0;
        float amplitude = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            value += OpenSimplex2.Noise3_ImproveXY(seeds[i], x * frequency, y * frequency, z * frequency) * amplitude;
            frequency *= parameters.Lacunarity;
            amplitude *= parameters.Persistence;
        }

        return value / topAmplitude;
    }

    public float Noise3_ImproveXZ(double x, double y, double z)
    {
        float value = 0.0f;
        double frequency = 1.0;
        float amplitude = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            value += OpenSimplex2.Noise3_ImproveXZ(seeds[i], x * frequency, y * frequency, z * frequency) * amplitude;
            frequency *= parameters.Lacunarity;
            amplitude *= parameters.Persistence;
        }

        return value / topAmplitude;
    }
}
