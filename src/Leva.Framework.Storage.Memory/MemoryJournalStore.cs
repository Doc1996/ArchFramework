namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Holds the mutable in-memory buffer for one journal and can be copied for storage sessions.
/// </summary>
internal sealed class MemoryJournalStore<TEntry> : IMemoryStoreBuffer
{
	private readonly Lock _lock = new();
	private readonly List<StorageEntry<TEntry>> _entries = [];
	private long _nextVersion = 1;

	public IMemoryStoreBuffer Clone()
	{
		lock (_lock)
		{
			var buffer = new MemoryJournalStore<TEntry>();
			buffer._entries.AddRange(_entries);
			buffer._nextVersion = _nextVersion;

			return buffer;
		}
	}

	public void CopyFrom(IMemoryStoreBuffer source)
	{
		var sourceBuffer = (MemoryJournalStore<TEntry>)source;
		var entries = sourceBuffer.Read(null, null);
		var nextVersion = sourceBuffer.GetNextVersion();

		lock (_lock)
		{
			_entries.Clear();
			_entries.AddRange(entries);
			_nextVersion = nextVersion;
		}
	}

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

	private long GetNextVersion()
	{
		lock (_lock)
			return _nextVersion;
	}
}
