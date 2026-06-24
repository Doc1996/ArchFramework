using Leva.Framework.Core;
using Microsoft.Data.Sqlite;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Owns a SQLite connection and transaction until changes are committed or rolled back.
/// </summary>
public sealed class SqliteStorageSession : IStorageSession
{
	private readonly SqliteStorageDatabase _database;
	private bool _isCompleted;

	internal SqliteStorageSession(
		SqliteStorageDatabase database,
		SqliteConnection connection,
		SqliteTransaction transaction
	)
	{
		ArgumentNullException.ThrowIfNull(database);
		ArgumentNullException.ThrowIfNull(connection);
		ArgumentNullException.ThrowIfNull(transaction);

		_database = database;
		Connection = connection;
		Transaction = transaction;
	}

	internal SqliteConnection Connection { get; }
	internal SqliteTransaction Transaction { get; }
	internal bool IsCompleted => _isCompleted;

	internal bool BelongsTo(SqliteStorageDatabase database) => ReferenceEquals(_database, database);

	public async Task<Result> CommitAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			if (_isCompleted)
				return CompletedResult("commit SQLite session");

			await Transaction.CommitAsync(token);
			_isCompleted = true;
			await CleanupAsync();

			return Result.Ok();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result.Fail(StorageErrors.Failed("commit SQLite session", ex.Message));
		}
	}

	public async Task<Result> RollbackAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			if (_isCompleted)
				return CompletedResult("rollback SQLite session");

			await Transaction.RollbackAsync(token);
			_isCompleted = true;
			await CleanupAsync();

			return Result.Ok();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result.Fail(StorageErrors.Failed("rollback SQLite session", ex.Message));
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_isCompleted)
			return;

		_isCompleted = true;
		try
		{
			await Transaction.RollbackAsync();
		}
		catch (Exception)
		{
			// Ignore rollback failure during dispose.
		}

		try
		{
			await CleanupAsync();
		}
		catch (Exception)
		{
			// Ignore cleanup failure during dispose.
		}
	}

	private async ValueTask CleanupAsync()
	{
		await Transaction.DisposeAsync();
		await Connection.DisposeAsync();
	}

	private static Result CompletedResult(string operation) =>
		Result.Fail(StorageErrors.Failed(operation, "The storage session is already completed."));
}
