using System.Text.Json;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Creates file repositories and journals under one root directory.
/// </summary>
public sealed class FileStorageProvider(string rootPath, JsonSerializerOptions? jsonOptions = null)
{
	private readonly FileStorageDatabase _database = new FileStorageDatabase(rootPath, jsonOptions);

	public FileRepository<TId, TValue> CreateRepository<TId, TValue>(string name)
		where TId : notnull => new(name, _database);

	public FileJournal<TEntry> CreateJournal<TEntry>(string name) => new(name, _database);
}
