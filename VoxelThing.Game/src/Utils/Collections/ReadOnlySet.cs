using System.Collections;

namespace VoxelThing.Game.Utils.Collections;

public class ReadOnlySet<T>(ISet<T> baseSet) : IReadOnlySet<T>, ISet<T>
{
    public int Count => baseSet.Count;
    public bool IsReadOnly => true;

    public IEnumerator<T> GetEnumerator() => baseSet.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => baseSet.GetEnumerator();

    public bool Contains(T item) => baseSet.Contains(item);
    public bool IsProperSubsetOf(IEnumerable<T> other) => baseSet.IsProperSubsetOf(other);
    public bool IsProperSupersetOf(IEnumerable<T> other) => baseSet.IsProperSupersetOf(other);
    public bool IsSubsetOf(IEnumerable<T> other) => baseSet.IsSubsetOf(other);
    public bool IsSupersetOf(IEnumerable<T> other) => baseSet.IsSupersetOf(other);
    public bool Overlaps(IEnumerable<T> other) => baseSet.Overlaps(other);
    public bool SetEquals(IEnumerable<T> other) => baseSet.SetEquals(other);

    void ICollection<T>.Add(T item) => throw new InvalidOperationException("Set is read only");
    public void ExceptWith(IEnumerable<T> other) => throw new InvalidOperationException("Set is read only");
    public void IntersectWith(IEnumerable<T> other) => throw new InvalidOperationException("Set is read only");
    bool ISet<T>.Add(T item) => throw new InvalidOperationException("Set is read only");
    public void Clear() => throw new InvalidOperationException("Set is read only");
    public void CopyTo(T[] array, int arrayIndex) => throw new InvalidOperationException("Set is read only");
    public bool Remove(T item) => throw new InvalidOperationException("Set is read only");
    public void SymmetricExceptWith(IEnumerable<T> other) => throw new InvalidOperationException("Set is read only");
    public void UnionWith(IEnumerable<T> other) => throw new InvalidOperationException("Set is read only");
}