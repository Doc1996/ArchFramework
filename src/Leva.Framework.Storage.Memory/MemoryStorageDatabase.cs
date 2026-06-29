namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Owns named in-memory repository and journal stores for one provider instance.
/// </summary>
internal sealed class MemoryStorageDatabase
{
	private readonly Lock _lock = new();
	private readonly Dictionary<string, object> _journals = new();
	private readonly Dictionary<string, object> _repositories = new();

	public MemoryRepositoryStore<TId, TModel> GetRepository<TId, TModel>(string name)
		where TId : notnull
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		var key = GetRepositoryKey<TId, TModel>(name);

		lock (_lock)
		{
			if (_repositories.TryGetValue(key, out var existing))
				return (MemoryRepositoryStore<TId, TModel>)existing;

			var store = new MemoryRepositoryStore<TId, TModel>(name);
			_repositories[key] = store;
			return store;
		}
	}

	public MemoryJournalStore<TEntry> GetJournal<TEntry>(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		var key = GetJournalKey<TEntry>(name);

		lock (_lock)
		{
			if (_journals.TryGetValue(key, out var existing))
				return (MemoryJournalStore<TEntry>)existing;

			var store = new MemoryJournalStore<TEntry>();
			_journals[key] = store;
			return store;
		}
	}

	private static string GetRepositoryKey<TId, TModel>(string name) =>
		$"{name}|{typeof(TId).AssemblyQualifiedName}|{typeof(TModel).AssemblyQualifiedName}";

	private static string GetJournalKey<TEntry>(string name) => $"{name}|{typeof(TEntry).AssemblyQualifiedName}";
}
