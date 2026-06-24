using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Holds file operations for one named repository folder.
/// </summary>
internal sealed class FileRepositoryStore<TId, TModel>
	where TId : notnull
{
	private readonly string _name;
	private readonly FileStorageDatabase _database;
	private readonly string _directory;

	public FileRepositoryStore(string name, FileStorageDatabase database, string directory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);

		_name = name;
		_database = database;
		_directory = directory;
	}

	public async Task<Result<StorageEntry<TModel>>> SaveAsync(
		TId id,
		TModel model,
		StorageVersion? expectedVersion,
		CancellationToken token
	)
	{
		try
		{
			Directory.CreateDirectory(_directory);

			var path = _database.GetRepositoryPath<TId, TModel>(_name, id);
			var storageId = _database.ToKey(id);
			var utcNow = DateTimeOffset.UtcNow;

			if (File.Exists(path))
			{
				var existing = await _database.ReadEntryAsync<TModel>(path, token);
				if (expectedVersion.HasValue && existing.Version != expectedVersion.Value)
					return Result<StorageEntry<TModel>>.Fail(
						StorageErrors.VersionConflict(_name, storageId, expectedVersion.Value, existing.Version)
					);

				var entry = existing with { Value = model, Version = existing.Version.Next(), UpdatedAt = utcNow };
				await _database.WriteEntryAsync(path, entry, token);
				return Result<StorageEntry<TModel>>.Ok(entry);
			}

			if (expectedVersion.HasValue)
				return Result<StorageEntry<TModel>>.Fail(StorageErrors.NotFound(_name, storageId));

			var entry = new StorageEntry<TModel>(model, new StorageVersion(1), utcNow, utcNow);
			await _database.WriteEntryAsync(path, entry, token);
			return Result<StorageEntry<TModel>>.Ok(entry);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<StorageEntry<TModel>>.Fail(
				StorageErrors.Failed($"save file repository '{_name}'", ex.Message)
			);
		}
	}

	public async Task<Result<StorageEntry<TModel>>> LoadAsync(TId id, CancellationToken token)
	{
		try
		{
			var path = _database.GetRepositoryPath<TId, TModel>(_name, id);
			if (!File.Exists(path))
				return Result<StorageEntry<TModel>>.Fail(StorageErrors.NotFound(_name, _database.ToKey(id)));

			var entry = await _database.ReadEntryAsync<TModel>(path, token);
			return Result<StorageEntry<TModel>>.Ok(entry);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<StorageEntry<TModel>>.Fail(
				StorageErrors.Failed($"load file repository '{_name}'", ex.Message)
			);
		}
	}

	public async Task<Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>> LoadAllAsync(CancellationToken token)
	{
		try
		{
			var entries = new Dictionary<TId, StorageEntry<TModel>>();
			if (!Directory.Exists(_directory))
				return Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>.Ok(entries);

			foreach (var file in Directory.EnumerateFiles(_directory, "*.json"))
			{
				token.ThrowIfCancellationRequested();
				var key = _database.GetFileKey(file);
				entries[_database.FromKey<TId>(key)] = await _database.ReadEntryAsync<TModel>(file, token);
			}

			return Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>.Ok(entries);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<IReadOnlyDictionary<TId, StorageEntry<TModel>>>.Fail(
				StorageErrors.Failed($"load file repository '{_name}'", ex.Message)
			);
		}
	}

	public Result<bool> Exists(TId id) =>
		Result<bool>.Ok(File.Exists(_database.GetRepositoryPath<TId, TModel>(_name, id)));

	public async Task<Result> DeleteAsync(TId id, StorageVersion? expectedVersion, CancellationToken token)
	{
		try
		{
			var path = _database.GetRepositoryPath<TId, TModel>(_name, id);
			var storageId = _database.ToKey(id);

			if (!File.Exists(path))
				return Result.Fail(StorageErrors.NotFound(_name, storageId));

			if (expectedVersion.HasValue)
			{
				var existing = await _database.ReadEntryAsync<TModel>(path, token);
				if (existing.Version != expectedVersion.Value)
					return Result.Fail(
						StorageErrors.VersionConflict(_name, storageId, expectedVersion.Value, existing.Version)
					);
			}

			File.Delete(path);
			return Result.Ok();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result.Fail(StorageErrors.Failed($"delete file repository '{_name}'", ex.Message));
		}
	}
}
