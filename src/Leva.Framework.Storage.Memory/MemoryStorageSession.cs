using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Owns an isolated in-memory database copy until changes are committed or rolled back.
/// </summary>
public sealed class MemoryStorageSession : IStorageSession
{
	private readonly MemoryStorageDatabase _rootDatabase;
	private bool _isCompleted;

	internal MemoryStorageSession(MemoryStorageDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		_rootDatabase = database;
		Database = database.Clone();
	}

	internal MemoryStorageDatabase Database { get; }
	internal bool IsCompleted => _isCompleted;

	internal bool BelongsTo(MemoryStorageDatabase database) => ReferenceEquals(_rootDatabase, database);

	public Task<Result> CommitAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		if (_isCompleted)
			return CompletedResult("commit in-memory session");

		_rootDatabase.ReplaceWith(Database);
		_isCompleted = true;
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> RollbackAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		if (_isCompleted)
			return CompletedResult("rollback in-memory session");

		_isCompleted = true;
		return Task.FromResult(Result.Ok());
	}

	public ValueTask DisposeAsync()
	{
		_isCompleted = true;
		return ValueTask.CompletedTask;
	}

	private static Task<Result> CompletedResult(string operation) =>
		Task.FromResult(Result.Fail(StorageErrors.Failed(operation, "The storage session is already completed.")));
}
