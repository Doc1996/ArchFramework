namespace Leva.Framework.Storage.Memory;

/// <summary>
/// Owns named in-memory repository and journal store buffers for one provider instance.
/// </summary>
internal sealed class MemoryStorageDatabase
{
	private readonly Lock _lock = new();
	private readonly Dictionary<string, IMemoryStoreBuffer> _journals = new();
	private readonly Dictionary<string, IMemoryStoreBuffer> _repositories = new();

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

	public MemoryStorageDatabase Clone()
	{
		var database = new MemoryStorageDatabase();
		lock (_lock)
		{
			foreach (var journal in _journals)
				database._journals[journal.Key] = journal.Value.Clone();

			foreach (var repository in _repositories)
				database._repositories[repository.Key] = repository.Value.Clone();
		}

		return database;
	}

	internal void ReplaceWith(MemoryStorageDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);
		var replacement = database.Clone();

		lock (_lock)
		{
			_journals.Clear();
			_repositories.Clear();

			foreach (var journal in replacement._journals)
				_journals[journal.Key] = journal.Value;

			foreach (var repository in replacement._repositories)
				_repositories[repository.Key] = repository.Value;
		}
	}

	private static string GetRepositoryKey<TId, TModel>(string name) =>
		$"{name}|{typeof(TId).AssemblyQualifiedName}|{typeof(TModel).AssemblyQualifiedName}";

	private static string GetJournalKey<TEntry>(string name) => $"{name}|{typeof(TEntry).AssemblyQualifiedName}";
}
