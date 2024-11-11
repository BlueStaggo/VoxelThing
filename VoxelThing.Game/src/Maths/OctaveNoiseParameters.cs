namespace VoxelThing.Game.Maths;

public readonly struct OctaveNoiseParameters
{
    public OctaveNoiseParameters() { }

    public double Lacunarity { get; init; } = 2.0;
    public float Persistence { get; init; } = 0.5f;
}
