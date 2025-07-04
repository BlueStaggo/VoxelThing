using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace VoxelThing.Game.Utils.Collections;

public class BiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    private readonly Dictionary<TKey, TValue> forward;
    private readonly Dictionary<TValue, TKey> reverse;

    public BiDictionary()
    {
        forward = new();
        reverse = new();
    }
    
    public BiDictionary(IDictionary<TKey, TValue> dictionary)
    {
        forward = dictionary.ToDictionary();
        reverse = dictionary
            .Select(kv => new KeyValuePair<TValue, TKey>(kv.Value, kv.Key))
            .ToDictionary();
    }

    public BiDictionary(List<TValue> list)
    {
        if (typeof(TKey) != typeof(int))
            throw new InvalidOperationException("Cannot make BiDictionary from list without int keys");

        forward = list
            .Select((value, i) => new KeyValuePair<TKey, TValue>((TKey)(object)i, value))
            .ToDictionary();
        reverse = list
            .Select((value, i) => new KeyValuePair<TValue, TKey>(value, (TKey)(object)i))
            .ToDictionary();
    }

    public int Count => forward.Count;
    public bool IsReadOnly => false;
    public ICollection<TKey> Keys => forward.Keys;
    public ICollection<TValue> Values => reverse.Keys;

    public TValue this[TKey index]
    {
        get => forward[index];
        set => Set(index, value);
    }
    
    public TKey KeyOf(TValue value) => reverse[value];

    public void Set(TKey key, TValue value)
    {
        forward[key] = value;
        reverse[value] = key;
    }
    
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Add(TKey key, TValue value)
    {
        forward.Add(key, value);
        reverse.Add(value, key);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
        => forward.Remove(item.Key) && reverse.Remove(item.Value);

    public bool Remove(TKey key)
        => forward.Remove(key, out TValue? value) && reverse.Remove(value);

    public void Clear()
    {
        forward.Clear();
        reverse.Clear();
    }
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => forward.TryGetValue(key, out value);

    public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key)
        => reverse.TryGetValue(value, out key);

    public bool Contains(KeyValuePair<TKey, TValue> item) => forward.ContainsKey(item.Key);
    public bool ContainsKey(TKey key) => forward.ContainsKey(key);
    public bool ContainsValue(TValue value) => reverse.ContainsKey(value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => forward.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection)forward).CopyTo(array, arrayIndex);
}