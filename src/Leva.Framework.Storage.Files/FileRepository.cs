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

	public async Task<Result<StorageEntry<TModel>>> SaveAsync(
		TId id,
		TModel model,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		return await _store.SaveAsync(id, model, expectedVersion, token);
	}

	public async Task<Result<StorageEntry<TModel>>> LoadAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return await _store.LoadAsync(id, token);
	}

	public async Task<Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>> LoadAllAsync(
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		return await _store.LoadAllAsync(token);
	}

	public Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(_store.Exists(id));
	}

	public async Task<Result> DeleteAsync(
		TId id,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		return await _store.DeleteAsync(id, expectedVersion, token);
	}
}
