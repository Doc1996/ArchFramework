namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Creates in-memory repositories and journals for one provider instance.
/// </summary>
public sealed class MemoryStorageProvider
{
	private readonly MemoryStorageDatabase _database = new();

	public MemoryRepository<TId, TValue> CreateRepository<TId, TValue>(string name)
		where TId : notnull => new(name, _database);

	public MemoryJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);
}
