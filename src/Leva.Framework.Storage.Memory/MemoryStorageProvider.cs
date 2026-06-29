namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Creates in-memory repositories and journals.
/// </summary>
public sealed class MemoryStorageProvider
{
	private readonly MemoryStorageDatabase _database = new();

	public IRepository<TId, TValue> CreateRepository<TId, TValue>(string name)
		where TId : notnull => new MemoryRepository<TId, TValue>(name, _database);

	public IJournal<TEntry> CreateJournal<TEntry>(string name) => new MemoryJournal<TEntry>(name, _database);
}
