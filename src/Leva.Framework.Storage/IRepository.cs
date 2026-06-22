using Leva.Framework.Core;

namespace Leva.Framework.Storage;

/// <summary>
/// Stores and loads keyed models without exposing provider-specific persistence details.
/// </summary>
public interface IRepository<TId, TModel>
	where TId : notnull
{
	Task<Result<StorageEntry<TModel>>> SaveAsync(
		TId id,
		TModel model,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	);

	Task<Result<StorageEntry<TModel>>> LoadAsync(TId id, CancellationToken token = default);
	Task<Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>> LoadAllAsync(CancellationToken token = default);

	Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default);
	Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default);
}
