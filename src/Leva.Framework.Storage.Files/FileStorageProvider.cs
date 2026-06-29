using System.Text.Json;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Creates file repositories and journals under one root folder.
/// </summary>
public sealed class FileStorageProvider(string rootDirectory, JsonSerializerOptions? jsonOptions = null)
{
	private readonly FileStorageDatabase _database = new(rootDirectory, jsonOptions);

	public IRepository<TId, TValue> CreateRepository<TId, TValue>(string name)
		where TId : notnull => new FileRepository<TId, TValue>(name, _database);

	public IJournal<TEntry> CreateJournal<TEntry>(string name) => new FileJournal<TEntry>(name, _database);
}
