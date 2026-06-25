namespace Leva.Framework.Core;

/// <summary>
/// Provides a small thread-safe list helper for synchronized framework collections.
/// </summary>
public sealed class SyncList<T>
{
	private readonly Lock _lock = new();
	private readonly List<T> _items = [];

	public IReadOnlyList<T> List()
	{
		lock (_lock)
			return _items.ToList();
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

	public void Add(T item)
	{
		lock (_lock)
			_items.Add(item);
	}

	public void Clear()
	{
		lock (_lock)
			_items.Clear();
	}
}
