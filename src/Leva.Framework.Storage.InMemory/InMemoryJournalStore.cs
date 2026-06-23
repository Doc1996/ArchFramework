namespace Leva.Framework.Storage.InMemory;

/// <summary>
/// Holds the mutable in-memory buffer for one journal and can be cloned for storage sessions.
/// </summary>
internal sealed class InMemoryJournalStore<TEntry> : IInMemoryStoreBuffer
{
	private readonly Lock _lock = new();
	private readonly List<StorageEntry<TEntry>> _entries = new();
	private long _nextVersion = 1;

	public IInMemoryStoreBuffer Clone()
	{
		lock (_lock)
		{
			var storeBuffer = new InMemoryJournalStore<TEntry>();
			storeBuffer._entries.AddRange(_entries);
			storeBuffer._nextVersion = _nextVersion;

			return storeBuffer;
		}
	}

	public StorageEntry<TEntry> Append(TEntry entry)
	{
		lock (_lock)
		{
			var utcNow = DateTimeOffset.UtcNow;
			var entry = new StorageEntry<TEntry>(entry, new StorageVersion(_nextVersion), utcNow, utcNow);

			_entries.Add(entry);
			_nextVersion++;
			return entry;
		}
	}

	public IReadOnlyList<StorageEntry<TEntry>> Read(StorageVersion? afterVersion, int? limit)
	{
		lock (_lock)
		{
			IEnumerable<StorageEntry<TEntry>> query = _entries;

			if (afterVersion.HasValue)
				query = query.Where(x => x.Version.Value > afterVersion.Value.Value);
			if (limit.HasValue)
				query = query.Take(limit.Value);

			return query.ToArray();
		}
	}
}
