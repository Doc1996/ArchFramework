using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Stores keyed values in memory for the lifetime of the owning provider instance.
/// </summary>
internal sealed class MemoryRepository<TId, TValue> : IRepository<TId, TValue>
	where TId : notnull
{
	private readonly string _name;
	private readonly MemoryRepositoryStore<TId, TValue> _store;

	internal MemoryRepository(string name, MemoryStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);

		_name = name;
		_store = database.GetRepository<TId, TValue>(name);
	}

	public Task<Result<StorageEntry<TValue>>> SaveAsync(
		TId id,
		TValue value,
		StorageVersion? expectedVersion = null,
		CancellationToken token = default
	)
	{
		token.ThrowIfCancellationRequested();
		var storedEntry = _store.Save(id, value, expectedVersion);
		return Task.FromResult(storedEntry);
	}

	public Task<Result<StorageEntry<TValue>>> LoadAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var storedEntry = _store.Load(id);

		if (storedEntry.HasValue)
			return Task.FromResult(Result<StorageEntry<TValue>>.Ok(storedEntry.Value));

		var key = id.ToString() ?? string.Empty;
		var error = StorageErrors.NotFound(_name, key);
		return Task.FromResult(Result<StorageEntry<TValue>>.Fail(error));
	}

	public Task<Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var storedEntries = _store.LoadAll();
		return Task.FromResult(Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>.Ok(storedEntries));
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
