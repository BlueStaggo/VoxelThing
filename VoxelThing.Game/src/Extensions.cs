using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using MemoryPack;

namespace VoxelThing.Game;

public static class Extensions
{
// Shut up C#, I know what I'm doing. It may look ugly but it works.
#pragma warning disable CS8600
#pragma warning disable CS8603
    // Only use this for sending vertex data or doing
    // anything that doesn't require writing to the list.
    public static T[] GetInternalArray<T>(this List<T> list)
        => !RuntimeFeature.IsDynamicCodeCompiled
            ? list.ToArray()
            : (T[])list.GetType()
                .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)! // Shouldn't be null ;)
                .GetValue(list);

#pragma warning restore CS8600
#pragma warning restore CS8603

    public static void AddSorted<T>(this List<T> list, T item, IComparer<T> comparer)
    {
        if (list.Count == 0 || comparer.Compare(list[^1], item) <= 0)
        {
            list.Add(item);
            return;
        }
        
        if (comparer.Compare(list[0], item) >= 0)
        {
            list.Insert(0, item);
            return;
        }

        int index = list.BinarySearch(item, comparer);
        if (index < 0)
            index = ~index;
        list.Insert(index, item);
    }

    private record NeverMemoryPackable;

    public static bool IsMemoryPackable(Type type)
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled) return true;
        MemoryPackWriter<IBufferWriter<byte>> dummyWriter = new();
        return dummyWriter.GetFormatter(typeof(NeverMemoryPackable)).GetType() != dummyWriter.GetFormatter(type).GetType();
    }
}