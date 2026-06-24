using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Owns an isolated file storage directory copy until changes are committed or rolled back.
/// </summary>
public sealed class FileStorageSession : IStorageSession
{
	private readonly FileStorageDatabase _rootDatabase;
	private bool _isCompleted;

	internal FileStorageSession(FileStorageDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		_rootDatabase = database;
		Database = database.Clone();
	}

	internal FileStorageDatabase Database { get; }
	internal bool IsCompleted => _isCompleted;

	internal bool BelongsTo(FileStorageDatabase database) => ReferenceEquals(_rootDatabase, database);

	public Task<Result> CommitAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			if (_isCompleted)
				return CompletedResult("commit file session");

			_rootDatabase.ReplaceWith(Database);
			_isCompleted = true;
			Cleanup();

			return Task.FromResult(Result.Ok());
		}
		catch (Exception ex)
		{
			return Task.FromResult(Result.Fail(StorageErrors.Failed("commit file session", ex.Message)));
		}
	}

	public Task<Result> RollbackAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		try
		{
			if (_isCompleted)
				return CompletedResult("rollback file session");

			_isCompleted = true;
			Cleanup();
			return Task.FromResult(Result.Ok());
		}
		catch (Exception ex)
		{
			return Task.FromResult(Result.Fail(StorageErrors.Failed("rollback file session", ex.Message)));
		}
	}

	public ValueTask DisposeAsync()
	{
		_isCompleted = true;
		Cleanup();
		return ValueTask.CompletedTask;
	}

	private void Cleanup()
	{
		try
		{
			Database.Delete();
		}
		catch (Exception)
		{
			// Ignore cleanup failure after session completion.
		}
	}

	private static Task<Result> CompletedResult(string operation) =>
		Task.FromResult(Result.Fail(StorageErrors.Failed(operation, "The storage session is already completed.")));
}
