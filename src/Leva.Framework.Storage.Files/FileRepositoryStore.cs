using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Holds file operations for one named repository folder.
/// </summary>
internal sealed class FileRepositoryStore<TId, TValue>
	where TId : notnull
{
	private readonly string _name;
	private readonly FileStorageDatabase _database;
	private readonly string _directory;
	private readonly FileStorageRunner _runner;

	public FileRepositoryStore(string name, FileStorageDatabase database, string directory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);

		_name = name;
		_database = database;
		_directory = directory;
		_runner = new($"file repository '{name}'");
	}

	internal Task<Result<StorageEntry<TValue>>> SaveAsync(
		TId id,
		TValue value,
		StorageVersion? expectedVersion,
		CancellationToken token
	) =>
		_runner.RunAsync(
			"save",
			async () =>
			{
				Directory.CreateDirectory(_directory);
				var path = GetPath(id);
				var key = FileStorageDatabase.ToKey(id);
				var utcNow = DateTimeOffset.UtcNow;

				if (!File.Exists(path))
					return await SaveNewAsync(path, key, value, expectedVersion, utcNow, token);

				return await SaveExistingAsync(path, key, value, expectedVersion, utcNow, token);
			},
			token
		);

	internal Task<Result<StorageEntry<TValue>>> LoadAsync(TId id, CancellationToken token) =>
		_runner.RunAsync(
			"load",
			async () =>
			{
				var path = GetPath(id);
				if (!File.Exists(path))
					return Result<StorageEntry<TValue>>.Fail(
						StorageErrors.NotFound(_name, FileStorageDatabase.ToKey(id))
					);

				var entry = await _database.ReadEntryAsync<TValue>(path, token);
				return Result<StorageEntry<TValue>>.Ok(entry);
			},
			token
		);

	internal Task<Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>> LoadAllAsync(CancellationToken token) =>
		_runner.RunAsync(
			"load",
			async () =>
			{
				var entries = new Dictionary<TId, StorageEntry<TValue>>();
				if (!Directory.Exists(_directory))
					return Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>.Ok(entries);

				foreach (var file in Directory.EnumerateFiles(_directory, "*.json"))
				{
					token.ThrowIfCancellationRequested();

					var key = FileStorageDatabase.GetFileKey(file);
					var id = FileStorageDatabase.FromKey<TId>(key);

					entries[id] = await _database.ReadEntryAsync<TValue>(file, token);
				}

				return Result<IReadOnlyDictionary<TId, StorageEntry<TValue>>>.Ok(entries);
			},
			token
		);

	internal Task<Result<bool>> ExistsAsync(TId id, CancellationToken token) =>
		_runner.RunAsync("check", () => Task.FromResult(Result<bool>.Ok(File.Exists(GetPath(id)))), token);

	internal Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion, CancellationToken token) =>
		_runner.RunAsync(
			"delete",
			async () =>
			{
				var path = GetPath(id);
				var key = FileStorageDatabase.ToKey(id);

				if (!File.Exists(path))
					return Result.Fail(StorageErrors.NotFound(_name, key));

				if (expectedVersion.HasValue)
				{
					var existing = await _database.ReadEntryAsync<TValue>(path, token);
					if (existing.Version != expectedVersion.Value)
						return Result.Fail(
							StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version)
						);
				}

				File.Delete(path);
				return Result.Ok();
			},
			token
		);

	private async Task<Result<StorageEntry<TValue>>> SaveNewAsync(
		string path,
		string key,
		TValue value,
		StorageVersion? expectedVersion,
		DateTimeOffset utcNow,
		CancellationToken token
	)
	{
		if (expectedVersion.HasValue)
			return Result<StorageEntry<TValue>>.Fail(StorageErrors.NotFound(_name, key));

		var entry = new StorageEntry<TValue>(value, new StorageVersion(1), utcNow, utcNow);
		await _database.WriteEntryAsync(path, entry, token);
		return Result<StorageEntry<TValue>>.Ok(entry);
	}

	private async Task<Result<StorageEntry<TValue>>> SaveExistingAsync(
		string path,
		string key,
		TValue value,
		StorageVersion? expectedVersion,
		DateTimeOffset utcNow,
		CancellationToken token
	)
	{
		var existing = await _database.ReadEntryAsync<TValue>(path, token);
		if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
			return Result<StorageEntry<TValue>>.Fail(
				StorageErrors.VersionConflict(_name, key, expectedVersion.Value, existing.Version)
			);

		var entry = existing with { Value = value, Version = existing.Version.Next(), UpdatedAt = utcNow };
		await _database.WriteEntryAsync(path, entry, token);
		return Result<StorageEntry<TValue>>.Ok(entry);
	}

	private string GetPath(TId id) => _database.GetRepositoryPath<TId, TValue>(_name, id);
}
