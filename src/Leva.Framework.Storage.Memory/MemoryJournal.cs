using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Stores append-only journal entries in memory for the lifetime of the owning provider instance.
/// </summary>
public sealed class MemoryJournal<TEntry> : IJournal<TEntry>
{
	private readonly MemoryJournalStore<TEntry> _store;

	internal MemoryJournal(string name, MemoryStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		_store = database.GetJournal<TEntry>(name);
	}

	public Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry entry, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var storedEntry = _store.Append(entry);
		return Task.FromResult(Result<StorageEntry<TEntry>>.Ok(storedEntry));
	}

	public Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion = null,
		int? limit = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		if (limit is <= 0)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero.");

		var entries = _store.Read(afterVersion, limit);
		return Task.FromResult(Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries));
	}
}
