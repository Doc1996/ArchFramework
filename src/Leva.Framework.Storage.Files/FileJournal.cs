using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Stores append-only journal entries as ordered JSON files under a named journal folder.
/// </summary>
public sealed class FileJournal<TEntry> : IJournal<TEntry>
{
	private readonly FileJournalStore<TEntry> _store;

	internal FileJournal(string name, FileStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		_store = database.GetJournal<TEntry>(name);
	}

	public async Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry entry, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return await _store.AppendAsync(entry, token);
	}

	public async Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion = null,
		int? limit = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		if (limit is <= 0)
			throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero.");

		return await _store.ReadAsync(afterVersion, limit, token);
	}
}
