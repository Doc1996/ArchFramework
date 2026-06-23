using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Stores keyed models in memory for the lifetime of the owning provider instance.
/// </summary>
public sealed class MemoryRepository<TId, TModel> : IRepository<TId, TModel>
	where TId : notnull
{
	private readonly string _name;
	private readonly MemoryRepositoryStore<TId, TModel> _store;

	internal MemoryRepository(string name, MemoryStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);

		_name = name;
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
		var storedEntry = _store.Save(id, model, expectedVersion);
		return Task.FromResult(storedEntry);
	}

	public Task<Result<StorageEntry<TModel>>> LoadAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var storedEntry = _store.Load(id);

		if (storedEntry.HasValue)
			return Task.FromResult(Result<StorageEntry<TModel>>.Ok(storedEntry.Value));

		var key = id.ToString() ?? string.Empty;
		var error = StorageErrors.NotFound(_name, key);
		return Task.FromResult(Result<StorageEntry<TModel>>.Fail(error));
	}

	public Task<Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var storedEntries = _store.LoadAll();
		return Task.FromResult(Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>.Ok(storedEntries));
	}

	public Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(Result<bool>.Ok(_store.Exists(id)));
	}

	public Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return Task.FromResult(_store.Delete(id, expectedVersion));
	}
}
