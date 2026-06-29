namespace Leva.Framework.Core;

/// <summary>
/// Provides a small thread-safe dictionary helper for synchronized framework collections.
/// </summary>
public sealed class SyncDictionary<TKey, TValue>
	where TKey : notnull
{
	private readonly Lock _lock = new();
	private readonly Dictionary<TKey, TValue> _items;

	public SyncDictionary()
	{
		_items = new Dictionary<TKey, TValue>();
	}

	public SyncDictionary(IEqualityComparer<TKey>? comparer)
	{
		_items = new Dictionary<TKey, TValue>(comparer);
	}

	public IReadOnlyList<KeyValuePair<TKey, TValue>> Items()
	{
		lock (_lock)
			return _items.ToList();
	}

	public IReadOnlyList<TValue> Values()
	{
		lock (_lock)
			return _items.Values.ToList();
	}

	public int Count
	{
		get
		{
			lock (_lock)
				return _items.Count;
		}
	}

	public bool Any()
	{
		lock (_lock)
			return _items.Count > 0;
	}

	public bool Any(Func<TValue, bool> predicate)
	{
		lock (_lock)
			return _items.Values.Any(predicate);
	}

	public void Set(TKey key, TValue value)
	{
		lock (_lock)
			_items[key] = value;
	}

	public bool TryGet(TKey key, out TValue? value)
	{
		lock (_lock)
			return _items.TryGetValue(key, out value);
	}

	public TValue? GetOrDefault(TKey key)
	{
		lock (_lock)
			return _items.TryGetValue(key, out var value) ? value : default;
	}

	public bool Remove(TKey key)
	{
		lock (_lock)
			return _items.Remove(key);
	}

	public void Clear()
	{
		lock (_lock)
			_items.Clear();
	}

	public bool ClearAndHadAny()
	{
		lock (_lock)
		{
			var hadAny = _items.Count > 0;
			_items.Clear();
			return hadAny;
		}
	}
}
