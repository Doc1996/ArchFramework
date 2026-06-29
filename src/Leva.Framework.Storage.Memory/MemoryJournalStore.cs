namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Holds the mutable in-memory entries for one journal.
/// </summary>
internal sealed class MemoryJournalStore<TEntry>
{
	private readonly Lock _lock = new();
	private readonly List<StorageEntry<TEntry>> _entries = [];
	private long _nextVersion = 1;

	public StorageEntry<TEntry> Append(TEntry value)
	{
		lock (_lock)
		{
			var utcNow = DateTimeOffset.UtcNow;
			var entry = new StorageEntry<TEntry>(value, new StorageVersion(_nextVersion++), utcNow, utcNow);
			_entries.Add(entry);

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
