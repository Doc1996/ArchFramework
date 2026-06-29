using Leva.Framework.Core;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Stores keyed values in SQLite under a named repository.
/// </summary>
internal sealed class SqliteRepository<TId, TValue> : IRepository<TId, TValue>
	where TId : notnull
{
	private readonly SqliteRepositoryStore<TId, TValue> _store;

	internal SqliteRepository(string name, SqliteStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
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
		return _store.SaveAsync(id, value, expectedVersion, token);
	}

	public Task<Result<StorageEntry<TValue>>> LoadAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.LoadAsync(id, token);
	}

	public Task<Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>> LoadAllAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.LoadAllAsync(token);
	}

	public Task<Result<bool>> ExistsAsync(TId id, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.ExistsAsync(id, token);
	}

	public Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.DeleteAsync(id, expectedVersion, token);
	}
}
