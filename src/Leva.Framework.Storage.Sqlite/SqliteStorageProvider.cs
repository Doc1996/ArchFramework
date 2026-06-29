using System.Text.Json;

namespace Leva.Framework.Storage.Sqlite;

/// <summary>
/// Creates SQLite repositories and journals for one database file.
/// </summary>
public sealed class SqliteStorageProvider(string databasePath, JsonSerializerOptions? jsonOptions = null)
{
	private readonly SqliteStorageDatabase _database = new(databasePath, jsonOptions);

	public IRepository<TId, TValue> CreateRepository<TId, TValue>(string name)
		where TId : notnull => new SqliteRepository<TId, TValue>(name, _database);

	public IJournal<TEntry> CreateJournal<TEntry>(string name) => new SqliteJournal<TEntry>(name, _database);
}
