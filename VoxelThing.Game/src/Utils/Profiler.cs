using System.Runtime.CompilerServices;

namespace VoxelThing.Game.Utils;

public class Profiler(bool doMeasurements = true)
{
    public const bool CompletelyBlockProfiler = false;
    public const int PrintIndentWidth = 2;
    
    public readonly ProfilerEntry Root = new();
    private readonly Stack<ProfilerEntry> entryStack = [];
    private ProfilerEntry CurrentEntry => entryStack.TryPeek(out ProfilerEntry? entry) ? entry : Root;
    
    public bool DoMeasurements = doMeasurements; 

    public void Push(string key)
    {
        if (CompletelyBlockProfiler || !DoMeasurements) return;
        
        ProfilerEntry parent = CurrentEntry;
        if (!parent.Entries.TryGetValue(key, out ProfilerEntry? newEntry))
            newEntry = parent.Entries[key] = new();
        
        entryStack.Push(newEntry);
        newEntry.Start();
    }

    public void Pop()
    {
        if (CompletelyBlockProfiler || !DoMeasurements) return;

        ProfilerEntry currentEntry = CurrentEntry;
        currentEntry.Stop();
        entryStack.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopPush(string key)
    {
        Pop();
        Push(key);
    }

    public void Clear()
    {
        entryStack.Clear();
        Root.Entries.Clear();
        Root.TimeElapsed = 0;
        Root.Calls = 0;
        Root.Stopwatch.Reset();
    }

    public void Print(TextWriter? textWriter = null)
    {
        textWriter ??= Console.Out;
        foreach (string entryKey in Root.Entries.Keys.Order())
            Print(textWriter,  entryKey, Root.Entries[entryKey]);
    }

    private static void Print(TextWriter textWriter, string key, ProfilerEntry entry, int indent = 0)
    {
        textWriter.WriteLine($"{new string(' ', indent)}{key}: {entry.MeanTime}ms");
        foreach (string entryKey in entry.Entries.Keys.Order())
            Print(textWriter, entryKey, entry.Entries[entryKey], indent + PrintIndentWidth);
    }
}