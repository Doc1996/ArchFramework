using Leva.Framework.Core;

namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Creates in-memory repositories, journals, and storage sessions for one provider instance.
/// </summary>
public sealed class MemoryStorageProvider : IStorageSessionFactory
{
	private readonly MemoryStorageDatabase _database = new();

	public MemoryRepository<TId, TModel> CreateRepository<TId, TModel>(string name)
		where TId : notnull => new(name, _database);

	public MemoryRepository<TId, TModel> CreateRepository<TId, TModel>(string name, IStorageSession session)
		where TId : notnull
	{
		var memorySession = RequireSession(session);
		return new MemoryRepository<TId, TModel>(name, memorySession.Database);
	}

	public MemoryJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);

	public MemoryJournal<TEntry> CreateJournal<TEntry>(string name, IStorageSession session)
	{
		var memorySession = RequireSession(session);
		return new MemoryJournal<TEntry>(name, memorySession.Database);
	}

	public Task<Result<IStorageSession>> OpenAsync(CancellationToken token = default)
	{
		token.ThrowIfCancellationRequested();
		var session = new MemoryStorageSession(_database);
		return Task.FromResult(Result<IStorageSession>.Ok(session));
	}

	private MemoryStorageSession RequireSession(IStorageSession session)
	{
		if (session is not MemoryStorageSession memorySession)
			throw new ArgumentException("The session was not opened by the in-memory provider.", nameof(session));

		if (!memorySession.BelongsTo(_database))
			throw new ArgumentException(
				"The session was opened by a different in-memory storage provider.",
				nameof(session)
			);

		if (memorySession.IsCompleted)
			throw new InvalidOperationException("The storage session is already completed.");

		return memorySession;
	}
}
