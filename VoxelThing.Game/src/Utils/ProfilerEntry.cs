using System.Diagnostics;

namespace VoxelThing.Game.Utils;

public class ProfilerEntry
{
    public double MeanTime => TimeElapsed / Calls;
    public double TimeElapsed { get; internal set; }
    public ulong Calls { get; internal set; }
    
    internal readonly Stopwatch Stopwatch = new();
    
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