using Leva.Framework.Core;

namespace Leva.Framework.Storage.Files;

/// <summary>
/// Holds file operations for one named append-only journal folder.
/// </summary>
internal sealed class FileJournalStore<TEntry>
{
	private readonly string _name;
	private readonly FileStorageDatabase _database;
	private readonly string _directory;

	public FileJournalStore(string name, FileStorageDatabase database, string directory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(database);
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);

		_name = name;
		_database = database;
		_directory = directory;
	}

	public async Task<Result<StorageEntry<TEntry>>> AppendAsync(TEntry value, CancellationToken token)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			Directory.CreateDirectory(_directory);

			var latest = Directory.Exists(_directory)
				? Directory
					.EnumerateFiles(_directory, "*.json")
					.Select(FileStorageDatabase.ParseVersion)
					.Select(x => x.Value)
					.DefaultIfEmpty(0)
					.Max()
				: 0;

			var version = new StorageVersion(latest + 1);
			var utcNow = DateTimeOffset.UtcNow;
			var entry = new StorageEntry<TEntry>(value, version, utcNow, utcNow);

			await _database.WriteEntryAsync(_database.GetJournalPath<TEntry>(_name, version), entry, token);
			return Result<StorageEntry<TEntry>>.Ok(entry);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<StorageEntry<TEntry>>.Fail(
				StorageErrors.Failed($"append file journal '{_name}'", ex.Message)
			);
		}
	}

	public async Task<Result<IReadOnlyList<StorageEntry<TEntry>>>> ReadAsync(
		StorageVersion? afterVersion,
		int? limit,
		CancellationToken token
	)
	{
		try
		{
			token.ThrowIfCancellationRequested();
			var entries = new List<StorageEntry<TEntry>>();
			if (!Directory.Exists(_directory))
				return Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries);

			IEnumerable<(string Path, StorageVersion Version)> query = Directory
				.EnumerateFiles(_directory, "*.json")
				.Select(x => (Path: x, Version: FileStorageDatabase.ParseVersion(x)))
				.Where(x => !afterVersion.HasValue || x.Version.Value > afterVersion.Value.Value)
				.OrderBy(x => x.Version.Value);

			if (limit.HasValue)
				query = query.Take(limit.Value);

			foreach (var file in query)
			{
				token.ThrowIfCancellationRequested();
				entries.Add(await _database.ReadEntryAsync<TEntry>(file.Path, token));
			}

			return Result<IReadOnlyList<StorageEntry<TEntry>>>.Ok(entries);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			return Result<IReadOnlyList<StorageEntry<TEntry>>>.Fail(
				StorageErrors.Failed($"read file journal '{_name}'", ex.Message)
			);
		}
	}
}
