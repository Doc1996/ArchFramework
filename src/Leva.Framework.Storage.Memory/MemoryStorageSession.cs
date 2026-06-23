using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Uses an isolated in-memory database copy until changes are committed or rolled back.
/// </summary>
public sealed class MemoryStorageSession : IStorageSession
{
	private readonly MemoryStorageDatabase _rootDatabase;
	private bool _completed;

	internal MemoryStorageSession(MemoryStorageDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		_rootDatabase = database;
		Database = database.Clone();
	}

	internal MemoryStorageDatabase Database { get; }
	internal bool IsCompleted => _completed;

	internal bool BelongsTo(MemoryStorageDatabase database) => ReferenceEquals(_rootDatabase, database);

	public Task<Result> CommitAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		if (_completed)
			return CompletedResult("commit in-memory session");

		_rootDatabase.ReplaceWith(Database);
		_completed = true;
		return Task.FromResult(Result.Ok());
	}

	public Task<Result> RollbackAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		if (_completed)
			return CompletedResult("rollback in-memory session");

		_completed = true;
		return Task.FromResult(Result.Ok());
	}

	public ValueTask DisposeAsync()
	{
		_completed = true;
		return ValueTask.CompletedTask;
	}

	private static Task<Result> CompletedResult(string operation) =>
		Task.FromResult(Result.Fail(StorageErrors.Failed(operation, "The storage session is already completed.")));
}
