using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Stores keyed models as JSON files under a named repository folder.
/// </summary>
public sealed class FileRepository<TId, TModel> : IRepository<TId, TModel>
	where TId : notnull
{
	private readonly FileRepositoryStore<TId, TModel> _store;

	internal FileRepository(string name, FileStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		_store = database.GetRepository<TId, TModel>(name);
	}

	public Task<Result<StorageEntry<TModel>>> SaveAsync(
		TId id,
		TModel model,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		return _store.SaveAsync(id, model, expectedVersion, token);
	}

	public Task<Result<StorageEntry<TModel>>> LoadAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.LoadAsync(id, token);
	}

	public Task<Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.LoadAllAsync(token);
	}

	public Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(_store.Exists(id));
	}

	public Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.DeleteAsync(id, expectedVersion, token);
	}
}
