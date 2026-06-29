using System.Text.Json;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Creates SQLite repositories and journals for one database file.
/// </summary>
public sealed class SqliteStorageProvider(string databasePath, JsonSerializerOptions? jsonOptions = null)
{
	private readonly SqliteStorageDatabase _database = new(databasePath, jsonOptions);

	public SqliteRepository<TId, TModel> CreateRepository<TId, TModel>(string name)
		where TId : notnull => new(name, _database);

	public SqliteJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);
}
