using Leva.Framework.Core;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Stores keyed models in SQLite under a named repository.
/// </summary>
public sealed class SqliteRepository<TId, TModel> : IRepository<TId, TModel>
	where TId : notnull
{
	private readonly SqliteRepositoryStore<TId, TModel> _store;

	internal SqliteRepository(string name, SqliteStorageDatabase database)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		_store = database.GetRepository<TId, TModel>(name);
	}

	internal SqliteRepository(string name, SqliteStorageDatabase database, SqliteStorageSession session)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		ArgumentNullException.ThrowIfNull(session);

		_store = database.GetRepository<TId, TModel>(name, session);
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
		return _store.ExistsAsync(id, token);
	}

	public Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion = null, CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		return _store.DeleteAsync(id, expectedVersion, token);
	}
}
