using System.Diagnostics;

namespace VoxelThing.Game.Utils;

public class ProfilerEntry
{
    public double MeanTime => TimeElapsed / Calls;
    
    internal readonly Stopwatch Stopwatch = new();
    internal double TimeElapsed;
    internal ulong Calls;
    
    public readonly Dictionary<string, ProfilerEntry> Entries = [];

    internal void Start()
    {
        Stopwatch.Restart();
    }

    internal void Stop()
    {
        if (!Stopwatch.IsRunning) return;
        Stopwatch.Stop();
        TimeElapsed += Stopwatch.Elapsed.TotalMilliseconds;
        Calls++;
    }
}