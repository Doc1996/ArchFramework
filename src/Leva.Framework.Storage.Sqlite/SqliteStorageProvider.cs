using System.Text.Json;
using Leva.Framework.Core;
using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Creates SQLite repositories, journals, and storage sessions for one database file.
/// </summary>
public sealed class SqliteStorageProvider(string databasePath, JsonSerializerOptions? jsonOptions = null)
	: IStorageSessionFactory
{
	private readonly SqliteStorageDatabase _database = new(databasePath, jsonOptions);

	public SqliteRepository<TId, TModel> CreateRepository<TId, TModel>(string name)
		where TId : notnull => new(name, _database);

	public SqliteRepository<TId, TModel> CreateRepository<TId, TModel>(string name, IStorageSession session)
		where TId : notnull
	{
		var sqliteSession = RequireSession(session);
		return new SqliteRepository<TId, TModel>(name, _database, sqliteSession);
	}

	public SqliteJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);

	public SqliteJournal<TEntry> CreateJournal<TEntry>(string name, IStorageSession session)
	{
		var sqliteSession = RequireSession(session);
		return new SqliteJournal<TEntry>(name, _database, sqliteSession);
	}

	public async Task<Result<IStorageSession>> OpenAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			var connection = await _database.OpenConnectionAsync(token);
			var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token);
			var session = new SqliteStorageSession(_database, connection, transaction);

			return Result<IStorageSession>.Ok(session);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<IStorageSession>.Fail(StorageErrors.Unavailable("Sqlite", ex.Message));
		}
	}

	private SqliteStorageSession RequireSession(IStorageSession session)
	{
		if (session is not SqliteStorageSession sqliteSession)
			throw new ArgumentException("The session was not opened by the SQLite provider.", nameof(session));

		if (!sqliteSession.BelongsTo(_database))
			throw new ArgumentException(
				"The session was opened by a different SQLite storage provider.",
				nameof(session)
			);

		if (sqliteSession.IsCompleted)
			throw new InvalidOperationException("The storage session is already completed.");

		return sqliteSession;
	}
}
