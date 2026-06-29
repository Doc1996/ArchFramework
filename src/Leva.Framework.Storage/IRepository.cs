using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Stores and loads keyed values without exposing provider-specific persistence details.
/// </summary>
public interface IRepository<TId, TValue>
	where TId : notnull
{
	Task<Result<StorageEntry<TValue>>> SaveAsync(
		TId id,
		TValue value,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	);

	Task<Result<StorageEntry<TValue>>> LoadAsync(TId id, CancellationToken token = default);
	Task<Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>> LoadAllAsync(CancellationToken token = default);

	Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default);
	Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default);
}
