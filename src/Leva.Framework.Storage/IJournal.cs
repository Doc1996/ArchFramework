using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Stores append-only entries for durable history, audit trails, or replayable records.
/// </summary>
public interface IJournal<TEntry>
{
	Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry entry, CancellationToken token = default);

	Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion = null,
		int? limit = null,
		CancellationToken token = default
	);
}
